namespace PLNSC
{
    public static class DBConnection
    {
        public const string oraDBDev = "Data Source = (Description = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.7.82.53)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = ELLDEV)));User Id=ellipse;Password=Elldev9;";
        public const string oraDBPrd = "Data Source = (Description = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.7.82.53)(PORT = 1521))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = ELLPRD)));User Id=ellipse;Password=Ellprd8;";
    }
}
