using Microsoft.AspNetCore.Identity;
using Tessera.Constants;

namespace Tessera.Models.Authentication
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string PetSecurityAnswer { get; set; }
        public string CarSecurityAnswer { get; set; }
        public string JobSecurityAnswer { get; set; }
        public int Theme { get; set; }
    }

    public class AppUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Theme Theme { get; set; }
    }

}
