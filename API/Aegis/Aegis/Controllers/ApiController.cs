using Tessera.Models;
using Microsoft.AspNetCore.Mvc;
using Tessera.Constants;
using System.Security.Claims;
using Aegis.Services;

namespace Aegis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly BookService _bookService;

        public ApiController(AuthService authService, BookService bookService)
        {
            _authService = authService;
            _bookService = bookService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDefaultModel model)
        {
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
         * LOGIN HTTP POST
         * - Verify user credentials.
         ***************************************************/
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDefaultModel model)
        {
            var result = await _authService.LoginAsync(model);

            if (result.Succeeded)
            {
                var books = await _authService.GetBooks(model.Email);
                if (books != null)
                {
                    return Ok(new 
                    { 
                        Result = Keys.API_LOGIN_SUCC, 
                        Books = books 
                    });
                }
                else
                {
                    return Ok(new { Result = Keys.API_LOGIN_SUCC });
                }
            }

            return Unauthorized("Invalid login attempt");
        }




        [HttpPost("CreateOrg")]
        public async Task<IActionResult> CreateOrg([FromBody] BookModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string ownerId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
            {
                return Unauthorized();
            }

            try
            {
                var (success, errorMsg) = await _authService.CreateBookAsync(ownerId, model);
                if (success)
                {
                    var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var userOrgs = await _authService.GetBooks(userEmail);
                        if (userOrgs != null)
                        {
                            return Ok(new
                            {
                                Result = Keys.API_ORG_SUCC,
                                Books = userOrgs
                            });
                        }
                    }
                    return Ok(new { Result = Keys.API_ORG_SUCC });
                }
                else
                    return Ok(new { Errors = errorMsg });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create organization. Please try again later.");
            }

         }



        [HttpPost("FetchChapters")]
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

                var chapters = await _bookService.GetChapters(bookId);
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
    }
}