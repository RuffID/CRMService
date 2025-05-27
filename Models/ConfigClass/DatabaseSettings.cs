namespace CRMService.Models.ConfigClass
{
    public class DatabaseSettings
    {
        public const string CONNECTION_STRINGS = "DatabaseSettings";
        public string MySqlMainCRM { get; set; } = string.Empty;
        public string MySqlServerInfoCRM { get; set; } = string.Empty;
        public string MySqlVersion { get; set; } = string.Empty;
        public string Postgres { get; set; } = string.Empty;
        public ushort LimitForRetrievingEntitiesFromDb {  get; set; } 
    }
}