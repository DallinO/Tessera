namespace Tessera.Constants
{
    public static class Keys
    {
        public const string SQL_SERVER_ROOT = "Server=DESKTOP-K3CVC1F\\SQLEXPRESS;";
        public const string SQL_SERVER_TESSERA_TEST = "TestDbConnection";

        public const string API_BASE_URL = "";
        public const string API_LOGIN_SUCC = "Access Granted";
        public const string API_LOGIN_FAIL = "Access Denied";
        public const string API_ORG_SUCC = "Organization Created";
        public const string API_ORG_FAIL = "Unable To Create Organization";
        public const string API_REG_SUCC = "User Created Successfuly";
        public const string API_REG_FAIL = "Unable To Create User";
        public const string API_GENERIC_SUCC = "Generic Success";
        public const string API_GENERIC_FAIL = "Generic Fail";

        public const string API_REG_CODE_1 = "DuplicateUserName";

    }

    public enum View
    {
        Login,
        Register,
        CreateOrg
    }
}
