using System.ComponentModel.DataAnnotations;

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
