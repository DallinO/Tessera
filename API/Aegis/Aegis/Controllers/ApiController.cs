using Tessera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Aegis.Data;
using Tessera.Constants;
using System.Security.Claims;

namespace Aegis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TesseraDbContext _dbContext;

        public ApiController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TesseraDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDefaultModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { Result = Keys.API_REG_SUCC});
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDefaultModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

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

            var organization = new OrganizationBase
            {
                Name = model.Name,
                OwnerId = ownerId
            };

            _dbContext.Organizations.Add(organization);

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(organization);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create organization. Please try again later.");
            }

         }
    }
}