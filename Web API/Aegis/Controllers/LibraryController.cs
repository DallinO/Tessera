using Aegis.Data;
using Aegis.Services;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LibraryController> _logger;

        public LibraryController(BookService bookService, IConfiguration configuration, ILogger<LibraryController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TesseraDbContext dbContext)
        {
            _bookService = bookService;
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        /* ############### BOOK ############### */

        /****************************************
         * CREATE BOOK                   (CREATE)
         * - Create user book.
         ****************************************/
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
                bookId = int.Parse(CodeGen.GenerateNumberOfLength(9));
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


        /***************************************************
         * CHECKOUT BOOKS                             (READ)
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
            // Get user data.
            var (userId, response) = await GetUserDataAsync(ClaimsPrincipal.Current);

            if (response != null)
            {
                return BadRequest(response);
            }

            // Check if the user has a book.
            var bookId = await _dbContext.Library
                .Where(book => book.ScribeId == userId)
                .Select(book => book.Id)
                .FirstOrDefaultAsync();

            if (bookId != 0)
            {
                return Ok(new ApiBookResponse
                {
                    Success = true,
                    BookId = bookId
                });
            }

            return NotFound(new ApiBookResponse()
            { 
                Success = false,
                Errors = new List<string>()
                {
                    "No book associated with the user id"
                }

            });
        }





        /* ############# CHAPTER ############## */

        /***************************************************
         * CREATE CHAPTER                           (CREATE)
         * - 
         ***************************************************/
        [Authorize]
        [HttpPost("createchapter")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddChapter([FromBody] ApiChapterRequest request)
        {
            // Get user data.
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, request.BookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Create list based on type.
            var serviceResponse = await _bookService.AddChapterAsync(database, request);

            // Return status based on the saveResponse.
            if (serviceResponse.Success)
            {
                _logger.LogInformation("CHAPTER ADDED");
                return Ok(serviceResponse);
            }
            else
            {
                _logger.LogError("INTERNAL SERVER ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        /***************************************************
         * GET CHAPTER LIST GET                       (READ)
         * - Get books and author data from the database.
         ***************************************************/
        [Authorize]
        [HttpGet("getchapters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChapterList([FromQuery] int bookId)
        {
            // Get user data.
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get notification list.
            var chapters = await _bookService.GetChaptersAsync(database, bookId);

            if (chapters == null)
            {
                _logger.LogWarning("500 Internal Server Error: Failed to retrieve chapter list -\n" +
                    $"\tUser Id: {userId}\n" +
                    $"\tDatabase: {database}\n" +
                    $"\tBook Id: {bookId}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Internal Server Error"
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


        /***************************************************
         * DELETE CHAPTER                           (DELETE)
         * - Delete a chapter.
         ***************************************************/
        [Authorize]
        [HttpDelete("deletechapter")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteChapter([FromQuery] int bookId, int chapterId)
        {
            // Get user data.
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            var deleteResponse = await _bookService.DeleteChapterAsync(database, bookId, chapterId);

            if (deleteResponse == null)
            {
                _logger.LogWarning("500 Internal Server Error: Failed to delete chapter -\n" +
                    $"\tUser Id: {userId}\n" +
                    $"\tDatabase: {database}\n" +
                    $"\tBook Id: {bookId}");
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Internal Server Error"
                    }

                });
            }

            _logger.LogInformation("");
            return Ok(new ApiResponse
            {
                Success = true,
            });
        }





        /* ############# DOCUMENT ############# */

        /****************************************
         * GET DOCUMENT                    (READ)
         * - Get books and author data from the
         * database.
         ****************************************/
        [Authorize]
        [HttpGet("getdocument")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocument([FromQuery] int bookId, int chapterId)
        {
            // Get user data.
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var document = await _bookService.GetDocumentAsync(database, chapterId);

            // Verify list
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


        /****************************************
         * SAVE DOCUMENT                 (UPDATE)
         * - 
         ****************************************/
        [Authorize]
        [HttpPut("savedocument")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveDocument([FromBody] SaveDocumentRequest request)
        {
            // Get user data.
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, request.BookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Attempt to save the changes to the database.
            var saveResponse = await _bookService.SaveDocumentAsync(database, request);

            // Return status based on the saveResponse.
            if (saveResponse.Success)
            {
                _logger.LogInformation("CHAPTER ADDED");
                return Ok(saveResponse);
            }
            else
            {
                _logger.LogError("INTERNAL SERVER ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, saveResponse);
            }

        }





        /* ############### ROW ################ */

        /****************************************
         * ADD ROW                       (CREATE)
         * - 
         ****************************************/
        [Authorize]
        [HttpPost("addrow")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRow([FromBody] ApiRowRequest request)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, request.BookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.AddRowAsync(database, request);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiListResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: No Chapters Associated With Book"
                    }

                });
            }

            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(serviceResponse);
        }


        [Authorize]
        [HttpPost("addnotification")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddNotification([FromBody] NotificationEntity notification)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, notification.BookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.AddNotificationAsync(database, notification);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: No Chapters Associated With Book"
                    }

                });
            }

            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(serviceResponse);
        }

        [Authorize]
        [HttpGet("getnotifications")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNotifications([FromQuery] int bookId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.GetNotificationsAsync(database, bookId);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "404 Not Found: No Chapters Associated With Book"
                    }

                });
            }

            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(serviceResponse);
        }




        /****************************************
         * GET ROWS                        (READ)
         * - 
         ****************************************/
        [Authorize]
        [HttpGet("getrows")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRows([FromQuery] int bookId, int chapterId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var list = await _bookService.GetRowsAsync(database, chapterId);

            // Verify list
            if (list == null)
            {
                _logger.LogWarning($"404 Not Found - No chapters associated with book: {bookId}");
                return NotFound(new ApiListResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }

                });
            }

            // Return list.
            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(list);
        }

        /****************************************
         * UPDATE ROW                    (UPDATE)
         * - 
         ****************************************/
        [Authorize]
        [HttpPut("updaterow")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRow([FromBody] ApiRowRequest request)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, request.BookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.UpdateRowAsync(database, request);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiListResponse
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
                return Ok(serviceResponse);
            }
        }


        /****************************************
         * DELETE ROW                    (DELETE)
         * - 
         ****************************************/
        [Authorize]
        [HttpDelete("deleterow")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRow([FromQuery] int bookId, int chapterId, int rowId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var list = await _bookService.DeleteRowAsync(database, chapterId, rowId);

            // Verify list
            if (list == null)
            {
                _logger.LogWarning("CHAPTERS NOT FOUND");
                return NotFound(new ApiListResponse
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
                return Ok(list);
            }


        }






        /****************************************
         * GET ROWS                        (READ)
         * - 
         ****************************************/
        [Authorize]
        [HttpGet("getupcomingtasks")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUpcomingTasks([FromQuery] int bookId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var list = await _bookService.GetUpcomingTasksAsync(database, bookId);

            // Verify list
            if (list == null)
            {
                _logger.LogWarning($"404 Not Found - No chapters associated with book: {bookId}");
                return NotFound(new ApiUpcomingTasksResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }

                });
            }

            // Return list.
            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(list);
        }

        /****************************************
         * GET ROWS                        (READ)
         * - 
         ****************************************/
        [Authorize]
        [HttpGet("getupcomingevents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int bookId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var list = await _bookService.GetUpcomingEventsAsync(database, bookId);

            // Verify list
            if (list == null)
            {
                _logger.LogWarning($"404 Not Found - No chapters associated with book: {bookId}");
                return NotFound(new ApiUpcomingEventsResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }

                });
            }

            // Return list.
            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(list);
        }


        /****************************************
         * GET ROWS                        (READ)
         * - 
         ****************************************/
        [Authorize]
        [HttpGet("getprioritytasks")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPriorityTasks([FromQuery] int bookId)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.GetPriorityTasksAsync(database, bookId);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning($"404 Not Found - No chapters associated with book: {bookId}");
                return NotFound(new ApiPriorityTasksResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }

                });
            }

            // Return list.
            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(serviceResponse);
        }






        /****************************************
        * GET ROWS                        (READ)
        * - 
        ****************************************/
        [Authorize]
        [HttpGet("getdayevents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDayEvents([FromQuery] int bookId, DateOnly date)
        {
            // Get user data
            var (userId, database, response) = await GetUserDataAsync(ClaimsPrincipal.Current, bookId);

            if (response != null)
            {
                if (userId != null)
                    return BadRequest(response);
                else
                    return NotFound(response);
            }

            // Get list data.
            var serviceResponse = await _bookService.GetDayEventsAsync(database, bookId, date);

            // Verify list
            if (serviceResponse == null)
            {
                _logger.LogWarning($"404 Not Found - No chapters associated with book: {bookId}");
                return NotFound(new ApiCalendarResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }

                });
            }

            // Return list.
            _logger.LogInformation("CHAPTERS RETURNED");
            return Ok(serviceResponse);
        }




        /* ########## HELPER METHODS ########## */

        /****************************************
         * CHECK IS BOOK OWNER ASYNC
         * - 
         ****************************************/
        private async Task<bool> CheckIsBookOwnerAsync(int bookId, string userId)
        {
            var isOwner = await _dbContext.Library
            .AnyAsync(b => b.Id == bookId && b.ScribeId == userId);

            return isOwner;
        }


        /****************************************
         * GET USER DATA ASYNC
         * - 
         ****************************************/
        private async Task<(string?, ApiResponse?)> GetUserDataAsync(ClaimsPrincipal user)
        {
            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
            {
                _logger.LogWarning("400 Bad Request: User id missing or incorrectly formatted.");
                return (null, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request"
                    }
                });
            }

            // Return result.
            return (userId, null);
        }

        private async Task<(string?, string?, ApiResponse?)> GetUserDataAsync(ClaimsPrincipal user, int bookId)
        {
            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
            {
                _logger.LogWarning("400 Bad Request: User id missing or incorrectly formatted.");
                return (null, null, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request"
                    }
                });
            }

            // Fetch database string.
            var database = await _dbContext.Library
               .Where(b => b.Id == bookId && b.ScribeId == userId)
               .Select(b => b.Database)
               .FirstOrDefaultAsync();

            // Verify database string.
            if (database == null)
            {
                _logger.LogWarning($"404 Not Found: Database string for {userId} not found.");
                return (userId, null, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Not Found"
                    }
                });
            }

            // Return result.
            return (userId, database, null);
        }
    }
}