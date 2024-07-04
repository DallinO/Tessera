using Tessera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Aegis.Data;
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

        public ApiController(AuthService authService)
        {
            _authService = authService;
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

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDefaultModel model)
        {
            var result = await _authService.LoginAsync(model);

            if (result.Succeeded)
            {
                return Ok(new { Result = Keys.API_LOGIN_SUCC });
            }

            return Unauthorized("Invalid login attempt");
        }

        [HttpPost("CreateOrg")]
        public async Task<IActionResult> CreateOrg([FromBody] OrganizationModel model)
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
                var (success, errorMsg) = await _authService.CreateOrganizationAsync(ownerId, model);
                if (success)
                    return Ok(new { Result = Keys.API_ORG_SUCC });
                else
                    return Ok(new { Errors = errorMsg });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create organization. Please try again later.");
            }

         }
    }
}