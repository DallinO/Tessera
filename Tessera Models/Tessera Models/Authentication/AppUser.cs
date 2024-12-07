using Microsoft.AspNetCore.Identity;

namespace Tessera.Models.Authentication
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public class AppUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Database { get; set; }
        public string Email { get; set; }
    }

}
