using Microsoft.AspNetCore.Identity;
using Tessera.Models.Book;

namespace Tessera.Models.Authentication
{
    public class Scribe : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public class ScribeDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Database { get; set; }
        public string Email { get; set; }
    }

}
