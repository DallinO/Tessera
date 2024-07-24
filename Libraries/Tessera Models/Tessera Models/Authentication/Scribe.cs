using Microsoft.AspNetCore.Identity;

namespace Tessera.Models
{
    public class Scribe : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
