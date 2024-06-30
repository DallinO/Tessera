using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models
{
    public class OrganizationUser
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public Role Role { get; set; }
    }
}
