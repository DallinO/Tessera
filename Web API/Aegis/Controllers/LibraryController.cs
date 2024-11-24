using Aegis.Data;
using Aegis.Services;
using Aegis.SwaggerTest;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Tessera.CodeGenerators;
using Tessera.Constants;
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

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Check if the user has a book.
            var bookId = await _dbContext.Library
                .Where(book => book.ScribeId == userId)
                .Select(book => book.Id)
                .FirstOrDefaultAsync();

            return Ok(new ApiBookResponse
            {
                Success = true,
                BookId = bookId
            });

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
        /// <description>Retrieves the scribe ID from the document context.</description>
        /// </item>
        /// <item>
        /// <description>Builds a <see cref="BookEntity"/> object from the provided model data.</description>
        /// </item>
        /// <item>
        /// <description>Retrieves the scribe's email address from the document context and fetches the associated scribe.</description>
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

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Return conflict error if the book id exists.
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

            // Create new book.
            var book = new BookEntity
            {
                Id = bookId,
                ScribeId = userId,
                Database = "tessera-pm-01"
            };

            // Sync database.
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

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            bool isOwner = await CheckIsBookOwnerAsync(bookId, userId);

            if (!isOwner)
            {

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
        [HttpGet("getchapterlist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChapterList([FromQuery] int bookId)
        {
            _logger.LogInformation("FETCH CHAPTERS CALLED");

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Fetch database string.
            string database = await GetDatabaseAsync(bookId, userId);

            // Verify database string.
            if (database == null)
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

            // Get request list.
            var chapters = await _bookService.GetChapterListAsync(database, bookId);
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
        [HttpGet("getdocument")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocument([FromQuery] int bookId, int chapterId)
        {
            // Log entry.
            _logger.LogInformation("GET CHAPTER CALLED");

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Fetch database string.
            string database = await GetDatabaseAsync(bookId, userId);

            // Verify database string.
            if (database == null)
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

            // Get document data.
            var document = await _bookService.GetDocumentDataAsync(database, bookId, chapterId);

            // Verify document
            if (document == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiDocumentResponse
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
                return Ok(new ApiDocumentResponse
                {
                    Success = true,
                    Document = document
                });
            }
        }


        [Authorize]
        [HttpPatch("savedocument")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveDocument([FromBody] SaveDocumentRequest request)
        {
            // Validate model.
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("INVALID STATE");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "400 Rad Request"
                    }
                });
            }

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Fetch database string.
            string database = await GetDatabaseAsync(request.BookId, userId);

            // Verify database string.
            if (database == null)
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

            // Attempt to save the changes to the database.
            var response = await _bookService.SaveDocumentDataAsync(database, request);

            // Return status based on the response.
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

        /***************************************************
         * CREATE CHAPTER HTTP POST
         * - Verify scribe credentials.
         ***************************************************/
        [Authorize]
        [HttpPost("createchapter")]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(CheckInRequestExample))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddChapter([FromBody] AddChapterRequest request)
        {
            // Validate request model.
            if (string.IsNullOrEmpty(request.Title))
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

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
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

            // Fetch database string.
            string database = await GetDatabaseAsync(request.BookId, userId);

            // Verify database string.
            if (database == null)
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

            // Create document based on type.
            var response = await _bookService.AddChaptersAsync(database, request);

            // Return status based on the response.
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


        private async Task<bool> CheckIsBookOwnerAsync(int bookId, string userId)
        {
            var isOwner = await _dbContext.Library
            .AnyAsync(b => b.Id == bookId && b.ScribeId == userId);

            return isOwner;
        }


        private async Task<string?> GetDatabaseAsync(int bookId, string userId)
        {
            // Query for the database column property. Verify the user
            // is the owner of the book.
            var database = await _dbContext.Library
               .Where(b => b.Id == bookId && b.ScribeId == userId)
               .Select(b => b.Database)
               .FirstOrDefaultAsync();

            return database;
        }
    }
}