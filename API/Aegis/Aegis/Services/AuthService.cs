using Microsoft.AspNetCore.Identity;
using Tessera.Models;
using Aegis.Data;
using Tessera.Constants;
using Microsoft.EntityFrameworkCore;

namespace Aegis.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DbContextFactory _dbContextFactory;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, DbContextFactory dbContextFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDefaultModel model)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email
            };

            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<SignInResult> LoginAsync(LoginDefaultModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        }

        public async Task<(bool Success, string ErrorMessage)> CreateOrganizationAsync(string ownerId, OrganizationModel model)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext("TestDbConnection"))
            {
                // Check if an organization with the same name already exists
                bool organizationExists = await dbContext.Organizations.AnyAsync(o => o.Name == model.Name);
                if (organizationExists)
                {
                    return (false, "Organization Already Exists");
                }

                // Create the organization
                var organization = new OrganizationBase
                {
                    Name = model.Name
                };

                dbContext.Organizations.Add(organization);
                await dbContext.SaveChangesAsync();

                // Create the entry in the UserOrganization join table
                var userOrganization = new UserOrganization
                {
                    ApplicationUserId = ownerId,
                    OrganizationBaseId = organization.Id,
                    IsOwner = true
                };

                dbContext.UserOrganizations.Add(userOrganization);
                await dbContext.SaveChangesAsync();

                return (true, null);
            }
        }
    }
}
