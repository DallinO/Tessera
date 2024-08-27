using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Book
{
    public class RoleEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<PermissionsEntity> Permissions { get; set; }
        public ICollection<RoleCollectionEntity> RoleCollections { get; set; }
        public ICollection<RolePermissionsEntity> RolePermissions { get; set; }
        public ICollection<RoleCollectionRoles> RoleCollectionRoles { get; set; }

    }

    public class RoleCollectionEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<RoleCollectionRoles> RoleCollectionRoles { get; set; }
        public ICollection<RoleEntity> Roles { get; set; }
    }

    public class RolePermissionsEntity
    {
        [Key]
        public int RoleId { get; set; }
        [Key]
        public int PermissionID { get; set; }

        // Navigation properties
        public RoleEntity Role { get; set; }
        public PermissionsEntity Permission { get; set; }
    }

    public class RoleCollectionRoles
    {
        [Key]
        public int RoleCollectionId { get; set; }
        [Key]
        public int RoleId { get; set; }

        // Navigation properties
        public RoleCollectionEntity RoleCollection { get; set; }
        public RoleEntity Role { get; set; }

    }
}
