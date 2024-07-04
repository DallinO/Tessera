using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models
{
    public class OrganizationBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public ApplicationUser Owner { get; set; }
    }

    public class UserOrganization
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Guid OrganizationBaseId { get; set; }
        public OrganizationBase OrganizationBase { get; set; }
        public bool IsOwner { get; set; }
    }


    public class OrganizationFull : OrganizationBase
    {
        public List<OrganizationUser> Users { get; set; } = new List<OrganizationUser>();
        public RolesTemplate CurrentRoleTemplate { get; set; }
    }

    public class OrganizationModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Confirm name is required")]
        [Compare("Name", ErrorMessage = "Names do not match.")]
        public string ConfirmName { get; set; }
    }

}
