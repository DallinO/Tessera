//#define Unit_Tests

using Aegis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Tessera.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Aegis.Data;
using Swashbuckle.AspNetCore.Filters;
using Azure.Core;
using Tessera.Models.Book;
using Tessera.Constants;
using Microsoft.IdentityModel.Tokens;
using Tessera.Models.Chapter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Aegis.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

#if Unit_Tests
        private static int _testUserId = 1;
#endif

        public AuthenticationController(
            IConfiguration configuration,
            ILogger<AuthenticationController> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
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


        [HttpPost("report")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Report([FromBody] ReportDto report)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"400 Bad Request - Request model is invalid: {report}");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request - Test"
                    }
                });
            }

            var newReport = new ReportEntity()
            {
                Date = report.Date,
                Issue = report.Issue
            };

            _dbContext.Reports.Add(newReport);

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new ApiResponse
                {
                    Success = true
                });
            }
            catch (Exception ex)
            {
                // Return Internal Server Error (500)
                _logger.LogError($"500 Internal Server Error - Error: {ex}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { $"500 Internal Server Error - Error: {ex}" }
                });
            }
        }

        /* ########## AUTHENTICATION ########## */

        /****************************************
         * REGISTER                      (CREATE)
         * - Registers a new user.
         ****************************************/
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Log action
            _logger.LogInformation("Register called.");

            // Check Model State
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"400 Bad Request - Request model is invalid: {request}");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request - Test"
                    }
                });
            }

            // Check for existing scribe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning($"409 Conflict - User already exists: {request.Email}");
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "User Already Exists"
                    }
                });
            }

            // Create the new app user
#if Unit_Tests

            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                Id = _testUserId++.ToString()
            };

#else
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                PetSecurityAnswer = request.PetSecurityAnswer,
                CarSecurityAnswer = request.CarSecurityAnswer,
                JobSecurityAnswer = request.JobSecurityAnswer
            };
#endif

            try
            {
                // Create user.
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"200 OK - Registration for {user.Email} successful.");
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

                    _logger.LogWarning($"400 Bad Request - Registration for {user.Email} failed.");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                // Return Internal Server Error (500)
                _logger.LogError($"500 Internal Server Error - Error: {ex}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { $"500 Internal Server Error - Error: {ex}" }
                });
            }
        }


        /****************************************
         * LOGIN                           (READ)
         * - Verify login credentials.
         ****************************************/
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiLoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Log action
            _logger.LogInformation("Login called.");

            // Check Model State
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"400 Bad Request - Request model is invalid: {request}");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request"
                    }
                });
            }

            try
            {
                // Get scribe
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Authenticate scribe
                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    _logger.LogWarning($"401 Unauthorized - Access denied for {request.Email}.");
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Access Denied"
                        }
                    });
                }

                // Generate tokens
                JwtSecurityToken token = _tokenService.GenerateJwt(user.Id);
                var refreshToken = TokenService.GenerateRefreshToken();

                // Set scribe tokens
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(1);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Log success
                    _logger.LogInformation($"200 Ok - Login for {request.Email} successful");

                    // Return response
                    return Ok(new ApiLoginResponse
                    {
                        Success = true,
                        JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        RefreshToken = refreshToken,
                        AppUser = new AppUserDto()
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Theme = (Theme)user.Theme
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

                    _logger.LogWarning($"400 Bad Request - Login failed for {request.Email}.");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = errors
                    });
                }

            }
            catch (Exception ex)
            {
                // Return Internal Server Error (500)
                _logger.LogError($"500 Internal Server Error - Error: {ex}.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "Internal Server Error" }
                });
            }
        }


        [HttpGet("validatetoken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiLoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            try
            {
                // Validate the token and get the ClaimsPrincipal
                var principal = _tokenService.GetPrincipalFromExpiredToken(token);

                if (principal == null)
                {
                    return Unauthorized(new ApiResponse()
                    {
                        Success = false,
                        Errors = new List<string>() { $"{token} invalid" }
                    });
                }

                var expClaim = principal?.Claims.FirstOrDefault(c => c.Type == "exp");

                if (expClaim == null || !long.TryParse(expClaim.Value, out var expUnixTime))
                {
                    return Unauthorized(new ApiResponse()
                    {
                        Success = false,
                        Errors = new List<string>() { $"{token} invalid" }
                    });
                }

                var expiration = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).DateTime;

                // Extract user ID claim from the token
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse()
                    {
                        Success = false,
                        Errors = new List<string>() { $"{token} invalid token claims" }
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);

                // Authenticate scribe
                if (user == null)
                {
                    _logger.LogWarning($"401 Unauthorized - Access denied for {userId}.");
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Access Denied"
                        }
                    });
                }
                    
                _logger.LogInformation($"200 Ok - Login for {userId} successful");

                // Return response
                return Ok(new ApiLoginResponse
                {
                    Success = true,
                    Expiration = expiration,
                    AppUser = new AppUserDto()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Theme = (Theme)user.Theme
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = $"Token validation failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    

        /****************************************
         * RESET PASSWORD                  (READ)
         * - 
         ****************************************/
        [HttpPut("resetpassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiLoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Log action
            _logger.LogInformation("Password reset called.");

            // Check Model State
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"400 Bad Request - Request model is invalid: {request}");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Bad Request"
                    }
                });
            }

            try
            {
                // Get user by email
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Check if user exists
                if (user == null)
                {
                    _logger.LogWarning($"401 Unauthorized - Access denied for {request.Email}.");
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Access Denied"
                        }
                    });
                }

                // Reset user's password
                var resetResult = await _userManager.RemovePasswordAsync(user);
                if (!resetResult.Succeeded)
                {
                    _logger.LogWarning($"Error removing password for {request.Email}");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Error removing existing password"
                        }
                    });
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);
                if (!addPasswordResult.Succeeded)
                {
                    _logger.LogWarning($"Error adding new password for {request.Email}");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = addPasswordResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                // Log success
                _logger.LogInformation($"Password successfully reset for {request.Email}");

                // Return successful response
                return Ok(new ApiResponse
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "An error occurred while resetting the password."
                    }
                });
            }
        }

        /****************************************
         * RESET PASSWORD                  (READ)
         * - 
         ****************************************/
        [HttpPut("updateuser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiLoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromBody] AppUserDto model)
        {
            // Log action
            _logger.LogInformation("Password reset called.");
            
            // Get user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Check if user exists
            if (user == null)
            {
                _logger.LogWarning($"User with email {model.Email} not found.");
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "User not found"
                    }
                });
            }

            // Update properties if they have changed
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Theme = (int)model.Theme;

            try
            {
                // Reset user's password
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Error updating user info for {model.Email}");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Error updating account information"
                        }
                    });
                }

                // Log success
                _logger.LogInformation($"Account information updated for {model.Email}");

                // Return successful response
                return Ok(new ApiResponse
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account information");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "An error occurred while updating account info"
                    }
                });
            }
        }


        /****************************************
         * DELETE USER                   (DELETE)
         * - 
         ****************************************/
        [Authorize]
        [HttpDelete("deleteuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser()
        {
            // Get email
            _logger.LogInformation("Delete user called.");

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
            {
                _logger.LogWarning("400 Bad Request: Unable to extract user id from claims.");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Token Claim Error",

                    }
                });
            }

            try
            {
                // Get user data 
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning($"400 Bad Request: Failed to retrieve user from id.");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                            {
                                "Bad Request"
                            }
                    });
                }

                // Remove the person from the database
                _dbContext.Users.Remove(user);

                // Save the changes
                await _dbContext.SaveChangesAsync();

                // Log success
                _logger.LogInformation($"200 OK - Successfully deleted user: {user.Email}.");
                return Ok(new ApiLoginResponse
                {
                    Success = true
                });
            }
            catch (Exception)
            {
                _logger.LogError("500 Internal Server Error: Failed to delete user.");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "Failed To Deactivate Account" }
                });
            }
        }


        /****************************************
         * REFRESH                       (UPDATE)
         * - Refreshes the Jwt token.
         ****************************************/
        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel model)
        {
            _logger.LogInformation("REFRESH CALLED");

            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);

            // Retrieve the user ID from the token’s claims instead of using Name
            var userId = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UNAUTHORIZED: User ID not found in token");
                return Unauthorized(new ApiLoginResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "401 Unauthorized"
                    }
                });
            }

            // Validate the user with their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || 
                user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("UNAUTHORIZED: Invalid refresh token or token expired");
                return Unauthorized(new ApiLoginResponse
                {
                    Success = false,
                    Errors = new List<string> { "401 Unauthorized" }
                });
            }

            // Pass the user ID to GenerateJwt
            var token = _tokenService.GenerateJwt(userId);

            _logger.LogInformation("REFRESH SUCCEEDED");

            return Ok(new ApiLoginResponse
            {
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                RefreshToken = model.RefreshToken
            });
        }


        /****************************************
         * LOGOUT                        (DELETE)
         * - Deletes users Jwt token.
         ****************************************/
        [Authorize]
        [HttpDelete("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            // Log action
            _logger.LogInformation("Logout called.");

            // Extract user id from token.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify user id.
            if (userId == null)
            {
                _logger.LogWarning("400 Bad Request: Unable to extract user id from claims.");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "Token Claim Error"
                    }
                });
            }

            try
            {
                // Get user data 
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning($"400 Bad Request: Failed to retrieve user from id.");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                            {
                                "Bad Request"
                            }
                    });
                }

                // Remove token
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);

                // Log success
                _logger.LogInformation($"200 OK - Logout for user {user.Email} Successful.");
                return Ok(new ApiLoginResponse
                {
                    Success = true
                });
            }
            catch (Exception)
            {
                _logger.LogError("500 Internal Server Error: Failed to logout user");
                // Return Internal Server Error (500)
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { "500 Internal Server Error: Failed To Logout AppUser" }
                });
            }
        }
    }
}