using Microsoft.AspNetCore.Identity;

namespace Tessera.Models.Authentication
{
    public class Scribe : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
