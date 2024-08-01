namespace Tessera.Models.Book
{
    public enum Permissions
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
