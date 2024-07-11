using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models.Book
{
    public class RoleEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class RoleCollectionEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RolePermissionsEntity
    {
        [Key]
        public int RoleId { get; set; }
        public int PermissionID { get; set; }
    }
}
