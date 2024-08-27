using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Book
{
    public class PermissionsEntity
    {
        [Key]
        public int Id { get; set; }
        public string Permission {  get; set; }

        // Navigation properties
        // Navigation property for many-to-many with RoleEntity
        public ICollection<RoleEntity> Roles { get; set; }
        public ICollection<RolePermissionsEntity> RolePermissions { get; set; }
    }


    public enum PermissionsE
    {
        // USER
        AddUser,
        EditUser,
        DeleteUser,
        ViewUsers,
        // ROLES
        CreateRole,
        EditRole,
        DeleteRole,
        AssignRole,
        // ORGANIZATION
        CreateOrganization,
        EditOrganization,
        DeleteOrganization,
        ViewOrganization,
        // PROJECT
        CreateProject,
        EditProject,
        DeleteProject,
        ViewProject,
        // TASK
        CreateTask,
        EditTask,
        DeleteTask,
        ViewTask,
        AssignTask,
        // REPOR
        GenerateReport,
        ViewReport,
        EditReport,
        DeleteReport,
        // FINANCIALS
        ViewFinancials,
        EditFinancials,
        ApproveFinancials,
        // SETTINGS
        EditSettings,
        ViewSettings,
        // DATA
        ExportData,
        ImportData,
        DeleteData
    }
}
