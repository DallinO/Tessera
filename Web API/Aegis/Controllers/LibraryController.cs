using Aegis.Data;
using Aegis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tessera.CodeGenerators;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;

namespace Aegis.Controllers
{
    [Route("api/library")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LibraryController> _logger;

        public LibraryController(BookService bookService, IConfiguration configuration, ILogger<LibraryController> logger, UserManager<Scribe> userManager, SignInManager<Scribe> signInManager, TesseraDbContext dbContext)
        {
            _bookService = bookService;
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        /***************************************************
         * CREATE BOOK HTTP POST
         * - Verify scribe credentials.
         ***************************************************/
        /// <summary>
        /// Creates a new book record in the database. This method performs the following operations:
        /// <list type="bullet">
        /// <item>
        /// <description>Validates the incoming model data.</description>
        /// </item>
        /// <item>
        /// <description>Retrieves the scribe ID from the chapter context.</description>
        /// </item>
        /// <item>
        /// <description>Builds a <see cref="BookEntity"/> object from the provided model data.</description>
        /// </item>
        /// <item>
        /// <description>Retrieves the scribe's email address from the chapter context and fetches the associated scribe.</description>
        /// </item>
        /// <item>
        /// <description>Attempts to create the book in the master and PM databases.</description>
        /// </item>
        /// <item>
        /// <description>If successful, retrieves the updated list of books for the scribe.</description>
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
        /// <description>Returns <see cref="Unauthorized()"/> if the scribe ID cannot be retrieved.</description>
        /// </item>
        /// <item>
        /// <description>Returns <see cref="StatusCode(StatusCodes.Status500InternalServerError, string)"/> if any errors occur during the process, with a detailed error message.</description>
        /// </item>
        /// <item>
        /// <description>Returns <see cref="Ok(object)"/> with the result and books list if the operation is successful.</description>
        /// </item>
        /// </list>
        /// </returns>
        
        [HttpPost("createbook")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBook()
        {
            Console.WriteLine("CREATE BOOK CALLED");
            
            // Get scribe id
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
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

            // Get Scribe object
            var scribe = await _userManager.FindByEmailAsync(email);
            if (scribe == null)
            {
                _logger.LogWarning("NOT FOUND");
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: User Does Not Exist"
                    }
                });
            }

            // Generate unique book code
            int count = 0;
            bool bookIdExists = false;
            int bookId;
            do
            {
                bookId = int.Parse(CodeGen.GenerateNineDigitId());
                bookIdExists = await _dbContext.Library.AnyAsync(o => o.Id == bookId);
                count++;
            }
            while (bookIdExists && count < 5);

            if (bookIdExists)
            {
                _logger.LogWarning("CONFLICT");
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "409 Conflict: Book Already Exists"
                    }
                });
            }
            else
            {
                var book = new BookEntity
                {
                    Id = bookId,
                    ScribeId = scribe.Id,
                    Database = "tessera-pm-01"
                };

                try
                {
                    _dbContext.Library.Add(book);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("BOOK CREATED");
                    return Ok(new ApiResponse
                    {
                        Success = true
                    });
                }
                catch (Exception)
                {
                    _logger.LogError("INTERNAL SERVER ERROR");
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string>
                            {
                                "500 Internal Server Error"
                            }

                        });
                }
            }

        }

        /***************************************************
         * CHECKOUT BOOKS HTTP GET
         * - Get books and author data from the database.
         ***************************************************/
        [Authorize]
        [HttpGet("getbookid")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckoutBooks()
        {
            _logger.LogInformation("CHECKOUT BOOKS CALLED");
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                _logger.LogWarning("UNAUTHORIZED");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "400 Bad Request: Email Claim Missing Or Incorrectly Formatted"
                    }
                });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("NOT FOUND");
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: User Does Not Exist"
                    }
                });
            }

            // Check if the scribe has any organizations assigned
            var bookId = await _dbContext.Library
                .Where(book => book.ScribeId == user.Id)
                .Select(book => book.Id)
                .FirstOrDefaultAsync();

            _logger.LogInformation("BOOKS CHECKED OUT");
            return Ok(new ApiBookResponse
            {
                Success = true,
                BookId = bookId
            });

        }


        [Authorize]
        [HttpDelete("deletebook")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBook([FromQuery] int bookId)
        {
            // Log the request
            _logger.LogInformation($"DeleteBook called with bookId: {bookId}");

            // Validate the bookId
            if (bookId <= 0)
            {
                return BadRequest(new ApiResponse 
                    { 
                        Success = false,
                        Errors = new List<string>()
                        { 
                            "Invalid book ID." 
                        } 
                    });
            }

            try
            {
                // Find the book in the database
                var book = await _dbContext.Library.FindAsync(bookId);

                if (book == null)
                {
                    // If the book does not exist, return a 404 Not Found
                    return NotFound(new ApiResponse { 
                        Success = false,
                        Errors = new List<string>()
                        { 
                            "Book not found." 
                        } 
                    });
                }

                // Remove the book from the context
                _dbContext.Library.Remove(book);
                await _dbContext.SaveChangesAsync(); // Commit the changes to the database

                // Return a success response
                return Ok(new ApiResponse
                {
                    Success = true
                });
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 Internal Server Error
                _logger.LogError($"Error deleting book: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        $"Error occured while deleting the book: {bookId}."
                    }
                });
            }
        }



        [Authorize]
        [HttpGet("getchapters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FetchChapters([FromQuery] int bookId)
        {
            _logger.LogInformation("FETCH CHAPTERS CALLED");

            var book = await _dbContext.Library
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                _logger.LogWarning("BOOK IS NULL");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "500 Internal Server Error: Failed To Retrieve Book Data"
                        }

                    });
            }

            var chapters = await _bookService.GetChaptersAsync(book.Database, book.Id);
            if (chapters == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "500 Internal Server Error: Failed To Retrieve Chapter Data"
                        }

                    });
            }

            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(new ApiChapterIndex
            {
                Success = true,
                Chapters = chapters
            });
        }


        [Authorize]
        [HttpGet("getchapterbody")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FetchChapters([FromQuery] int bookId, int chapterId)
        {
            _logger.LogInformation("GET CHAPTER CALLED");

            var book = await _dbContext.Library
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
            {
                _logger.LogWarning("BOOK IS NULL");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "500 Internal Server Error: Failed To Retrieve Book Data"
                        }

                    });
            }

            var chapter = await _bookService.GetChapterAsync(book.Database, book.Id);
            if (chapter == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiChapterData
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: No Chapters Associated With Book"
                    }

                });
            }
            else
            {
                _logger.LogInformation("CHAPTERS RETURNED");
                return Ok(new ApiChapterData
                {
                    Success = true,
                    Chapter = chapter
                });
            }
        }


        /***************************************************
         * CREATE CHAPTER HTTP POST
         * - Verify scribe credentials.
         ***************************************************/
        [Authorize]
        [HttpPost("createchapter")]
        public async Task<IActionResult> AddChapter([FromBody] AddChapterRequest chapter)
        {
            if (string.IsNullOrEmpty(chapter.Title))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "400 Rad Request: Chapter Title Cannot Be Null Or Empty."
                    }
                });
            }

            var book = await _dbContext.Library
                .FirstOrDefaultAsync(b => b.Id == chapter.BookId);

            if (book == null)
            {
                _logger.LogWarning("BOOK IS NULL");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "500 Internal Server Error: Failed To Retrieve Book Data"
                        }

                    });
            }

            var response = await _bookService.AddChaptersAsync(book.Database, chapter);
            if (response.Success)
            {
                _logger.LogInformation("CHAPTER ADDED");
                return Ok(response);
            }
            else
            {
                _logger.LogError("INTERNAL SERVER ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}