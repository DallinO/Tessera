namespace Tessera.Constants
{
    public static class Keys
    {
        public const string SQL_SERVER_ROOT = "Server=tcp:tessera-pm.database.windows.net,1433;";
        public const string SQL_SERVER_TESSERA_TEST = "DefaultConnection";
        public const string INIT_BOOK_SCRIPT = "InitializeBook.sql";

        public const string API_BASE_URL = "";
        public const string API_LOGIN_SUCC = "Access Granted";
        public const string API_LOGIN_FAIL = "Access Denied";
        public const string API_ORG_SUCC = "Organization Created";
        public const string API_ORG_FAIL = "Unable To Create Organization";
        public const string API_REG_SUCC = "User Created Successfuly";
        public const string API_REG_FAIL = "Unable To Create User";
        public const string API_GENERIC_SUCC = "Success";
        public const string API_GENERIC_FAIL = "Fail";

        public const string API_REG_CODE_1 = "DuplicateUserName";

    }

    public enum View
    {
        Login,
        Register,
        CreateOrg,
        SelectOrg
    }

    public enum Priority
    {
        Low,
        Moderate,
        High
    }

    public enum Status
    {
        Open,
        Pending,
        Closed
    }

    public enum LeafType
    {
        Document,
        List
    }

    public enum EventType
    {
        Work,
        School,
        Medical,
        Personal,
        Religious
    }

    public enum Theme
    {
        Default,
        Blue,
        Red,
        Orange,
        Green
    }


    public enum EntityType
    {
        Row,
        Event
    }



}
