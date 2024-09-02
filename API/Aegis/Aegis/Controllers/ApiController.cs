using Aegis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Tessera.Constants;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Aegis.Data;

namespace Aegis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly BookService _bookService;
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AuthService authService, BookService bookService, IConfiguration configuration, ILogger<ApiController> logger, UserManager<Scribe> userManager, SignInManager<Scribe> signInManager, TesseraDbContext dbContext)
        {
            _authService = authService;
            _bookService = bookService;
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }


        /***************************************************
         * LOGIN HTTP POST
         * - Verify user credentials.
         ***************************************************/
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            _logger.LogInformation("Login called");

            var user = await _userManager.FindByEmailAsync(model.Email);
            // Call the LoginAsync method
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }

            JwtSecurityToken token = GenerateJwt(model.Email);
            var refreshToken = GenerateRefreshToken();
                
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Login succeeded");

            return Ok(new LoginResponse
            {
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                RefreshToken = refreshToken
            });
        }

        /***************************************************
         * REGISTER HTTP POST
         * - Register new user.
         ***************************************************/
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDefaultModel model)
        {
            // Check Model State
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.Succeeded)
            {
                return Ok(new { Result = Keys.API_REG_SUCC});
            }

            return BadRequest(result.Errors);
        }


        /***************************************************
         * CHECKOUT BOOKS HTTP POST
         * - Get books and author data from the database.
         ***************************************************/
        [HttpPost("CheckoutBooks")]
        [Authorize]
        public async Task<IActionResult> CheckoutBooks([FromBody] LoginDefaultModel model)
        {
            var books = await _authService.GetBooks(model.Email);
            var author = await _authService.GetAuthor(model.Email);
            var response = new Dictionary<string, object>();

            if (books != null)
            {
                response["books"] = books;
            }

            if (author != null)
            {
                response["author"] = author;
            }

            return Ok(response);
        }


        /***************************************************
         * CREATE BOOK HTTP POST
         * - Verify user credentials.
         ***************************************************/
        /// <summary>
        /// Creates a new book record in the database. This method performs the following operations:
        /// <list type="bullet">
        /// <item>
        /// <description>Validates the incoming model data.</description>
        /// </item>
        /// <item>
        /// <description>Retrieves the user ID from the request context.</description>
        /// </item>
        /// <item>
        /// <description>Builds a <see cref="BookEntity"/> object from the provided model data.</description>
        /// </item>
        /// <item>
        /// <description>Retrieves the user's email address from the request context and fetches the associated scribe.</description>
        /// </item>
        /// <item>
        /// <description>Attempts to create the book in the master and PM databases.</description>
        /// </item>
        /// <item>
        /// <description>If successful, retrieves the updated list of books for the user.</description>
        /// </item>
        /// <item>
        /// <description>Returns an appropriate HTTP response based on the outcome of the operations, including error messages if applicable.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="model">The <see cref="BookModel"/> containing data for the new book.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that represents the outcome of the operation:
        /// <list type="bullet">
        /// <item>
        /// <description>Returns <see cref="BadRequest(ModelState)"/> if the model state is invalid.</description>
        /// </item>
        /// <item>
        /// <description>Returns <see cref="Unauthorized()"/> if the user ID cannot be retrieved.</description>
        /// </item>
        /// <item>
        /// <description>Returns <see cref="StatusCode(StatusCodes.Status500InternalServerError, string)"/> if any errors occur during the process, with a detailed error message.</description>
        /// </item>
        /// <item>
        /// <description>Returns <see cref="Ok(object)"/> with the result and books list if the operation is successful.</description>
        /// </item>
        /// </list>
        /// </returns>
        [HttpPost("CreateBook")]
        [Authorize]
        public async Task<IActionResult> CreateBook([FromBody] BookModel model)
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user id
            string ownerId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                return Unauthorized();
            }

            try
            {
                // Build Book Entity for row insertion
                var book = new BookEntity
                {
                    Title = model.Name,
                    ScribeId = ownerId
                };

                // Get user email
                var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve user email from request.");
                }

                // Get Scribe
                var scribe = await _authService.GetAuthor(userEmail);
                if (scribe == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve scribe from the database.");
                }

                // Attempt to create the Book
                var (masterSuccess, masterErrorMsg) = await _authService.CreateBookAsync(book);

                // On success
                if (masterSuccess)
                {
                    var books = await _authService.GetBooks(userEmail);
                    if (books != null)
                    {
                        return Ok(new
                        {
                            Result = Keys.API_ORG_SUCC,
                            Books = books
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            Result = Keys.API_ORG_SUCC
                        });
                    }

                }
                else
                {
                    var errors = new List<string>();
                    if (!masterSuccess) errors.Add(masterErrorMsg);
                    if (errors.Count == 0) errors.Add(Keys.API_GENERIC_FAIL);

                    return StatusCode(StatusCodes.Status500InternalServerError, new { Errors = errors });
                }
            }
            catch (Exception ex)
            {
                // Log the exception here if necessary
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



        [HttpPost("FetchChapters")]
        [Authorize]
        public async Task<IActionResult> FetchChapters([FromBody] string bookTitle)
        {
            if (string.IsNullOrEmpty(bookTitle))
            {
                return BadRequest("Database name cannot be null or empty.");
            }

            try
            {
                Guid? bookId = await _authService.GetBookIdByTitleAsync(bookTitle);
                if (bookId == null)
                {
                    return StatusCode(500, "Internal server error");
                }

                var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var scribe = await _authService.GetAuthor(userEmail);
                var chapters = await _bookService.GetChaptersAsync(scribe.Database, bookId);
                if (chapters == null || !chapters.Any())
                {
                    return NotFound("No chapters found.");
                }
                return Ok(chapters);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in FetchChapters: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /***************************************************
         * CREATE CHAPTER HTTP POST
         * - Verify user credentials.
         ***************************************************/
        [HttpPost("AddChapter")]
        [Authorize]
        public async Task<IActionResult> AddChapter([FromBody] AddChapterRequest request)
        {
            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest("Chapter name cannot be empty");
            }

            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var scribe = await _authService.GetAuthor(userEmail);

            var (status, message) = await _bookService.AddChaptersAsync(scribe.Database, request);

            if (status)
            {
                return Ok( new { Result = message });
            }
            else
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("ValidateToken")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized("Authorization header is missing.");
            }
            return Ok();
        }

        private JwtSecurityToken GenerateJwt(string username)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured")));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddSeconds(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var generator = RandomNumberGenerator.Create();

            generator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}