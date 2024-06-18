using Microsoft.AspNetCore.Identity;

namespace Aegis.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
