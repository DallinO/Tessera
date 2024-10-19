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
using Aegis.SwaggerTest;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.EntityFrameworkCore;

namespace Aegis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;


        public AuthenticationController(
            IConfiguration configuration, 
            ILogger<AuthenticationController> logger, 
            UserManager<Scribe> userManager, 
            SignInManager<Scribe> signInManager, 
            TesseraDbContext dbContext, 
            TokenService tokenService)
        {
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _tokenService = tokenService;
        }


        /***************************************************
         * REGISTER HTTP POST
         * - Register new scribe.
         ***************************************************/
        [HttpPost("Register")]
        [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            // Log action
            _logger.LogInformation("REGISTER CALLED");

            // Check Model State
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("INVALID STATE");
                return BadRequest( new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "400 Rad Request"
                    }
                });
            }

            // Check for existing scribe
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("CONFLICT");
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "409 Conflict: User Already Exists"
                    }
                });
            }

            // Create the new scribe
            var user = new Scribe
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("REGISTER SUCCEEDED");
                    return Ok(new ApiResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    List<string> errors = new();
                    foreach (var error in result.Errors)
                    {
                        errors.Add(error.Description);
                    }

                    _logger.LogWarning("REGISTER FAILED");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = errors
                    });
                }
            }
            catch (Exception)
            {
                // Return Internal Server Error (500)
                _logger.LogError("INTERNAL SERVER ERROR");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "500 Internal Server Error" }
                });
            }
            // Submit changes to Database
        }


        /***************************************************
         * LOGIN HTTP POST
         * - Verify scribe credentials.
         ***************************************************/
        [HttpPost("CheckIn")]
        [AllowAnonymous]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(CheckInRequestExample))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiLoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckIn([FromBody] LoginRequest model)
        {
            // Log action
            _logger.LogInformation("CHECK IN CALLED");

            // Check Model State
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("INVALID MODEL STATE");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "400 Bad Request"
                    }
                });
            }

            try
            {
                // Get scribe
                var user = await _userManager.FindByEmailAsync(model.Email);

                // Authenticate scribe
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    _logger.LogWarning("UNAUTHORIZED");
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "401 Unauthorized"
                        }
                    });
                }

                // Generate tokens
                JwtSecurityToken token = _tokenService.GenerateJwt(model.Email);
                var refreshToken = TokenService.GenerateRefreshToken();
            
                // Set scribe tokens
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Log success
                    _logger.LogInformation("CHECK IN SUCCEEDED");

                    // Return response
                    return Ok(new ApiLoginResponse
                    {
                        Success = true,
                        JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        RefreshToken = refreshToken,
                        Author = new ScribeDto() 
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email
                        }
                    });
                }
                else
                {
                    List<string> errors = new();
                    foreach (var error in result.Errors)
                    {
                        errors.Add(error.Description);
                    }

                    _logger.LogWarning("CHECK IN FAILED");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = errors
                    });
                }

            }
            catch (Exception)
            {
                // Return Internal Server Error (500)
                _logger.LogError("INTERNAL SERVER ERROR");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "500 Internal Server Error" }
                });
            }
        }


        /***************************************************
         * REVOKE HTTP DELETE
         * - Generates a Jwt token.
         ***************************************************/
        [Authorize]
        [HttpDelete("BanishScribe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BanishScibe()
        {
            // Get email
            _logger.LogInformation("BANISH SCRIBE CALLED");
            var email = HttpContext.User.Identity?.Name;
            if (email is null)
            {
                _logger.LogWarning("UNATHORIZED");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "401 Unauthorized"
                    }
                });
            }


            try
            {
                var scribe = await _userManager.FindByEmailAsync(email);
                if (scribe is null)
                {
                    // Log the not found error
                    _logger.LogWarning("NOT FOUND");
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "404 Not Found: Person does not exist" }
                    });
                }

                // Remove the person from the database
                _dbContext.Scribes.Remove(scribe);

                // Save the changes
                await _dbContext.SaveChangesAsync();

                // Log success
                _logger.LogInformation($"SUCCESS");

                // Return No Content (204) or OK (200)
                return Ok(new ApiLoginResponse
                {
                    Success = true
                });
            }
            catch (Exception)
            {
                _logger.LogError("INTERNAL SERVER ERROR");
                // Return Internal Server Error (500)
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "500 Internal Server Error: Failed to delete person" }
                });
            }
        }




        /***************************************************
         * REVOKE HTTP DELETE
         * - Generates a Jwt token.
         ***************************************************/
        [Authorize]
        [HttpDelete("Checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Checkout()
        {
            // Log action
            _logger.LogInformation("Checkout called");

            // Get email
            var username = HttpContext.User.Identity?.Name;
            if (username is null)
            {
                _logger.LogWarning("UNAUTHORIZED");
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "401 Unauthorized"
                        }
                });
            }

            try
            {
                // Get scribe 
                var user = await _userManager.FindByNameAsync(username);
                if (user is null)
                {
                    _logger.LogWarning("UNAUTHORIZED");
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                            {
                                "401 Unauthorized"
                            }
                    });
                }

                // Remove token
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);

                // Log success
                _logger.LogInformation("CHECKOUT SUCCEEDED");
                return Ok(new ApiLoginResponse
                {
                    Success = true
                });
            }
            catch (Exception)
            {
                _logger.LogError("INTERNAL SERVER ERROR");
                // Return Internal Server Error (500)
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "500 Internal Server Error: Failed To Checkout Scribe" }
                });
            }
        }

        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel model)
        {
            _logger.LogInformation("REFRESH CALLED");

            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);

            if (principal?.Identity?.Name is null)
            {
                _logger.LogWarning("UNAUTHORIZED");
                return Unauthorized(new ApiLoginResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "401 Unauthorized"
                    }
                });
            }

            var user = await _userManager.FindByNameAsync(principal.Identity.Name);

            if (user is null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("UNAUTHORIZED");
                return Unauthorized(new ApiLoginResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "401 Unauthorized"
                    }
                });
            }

            var token = _tokenService.GenerateJwt(principal.Identity.Name);

            _logger.LogInformation("REFRESH SUCCEEDED");

            return Ok(new ApiLoginResponse
            {
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                RefreshToken = model.RefreshToken
            });
        }

    }
}