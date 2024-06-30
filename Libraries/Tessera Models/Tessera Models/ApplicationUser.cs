using Microsoft.AspNetCore.Identity;

namespace Tessera.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> OwnedOrganizations { get; set; } = new List<int>();
        public List<int> OrganizationsJoined { get; set; } = new List<int>();
    }
}
