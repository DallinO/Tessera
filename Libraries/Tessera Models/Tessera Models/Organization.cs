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
        public int Id { get; set; }

        public string Name { get; set; }
        public string OwnerId { get; set; }
    }
    
    public class OrganizationFull
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
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
