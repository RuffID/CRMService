namespace CRMService.Models.ConfigClass
{
    public class OkdeskSettings
    {
        public const string OKDESK = "Okdesk";
        public string ApiToken { get; set; } = string.Empty;
        public ushort LimitForRetrievingEntitiesFromApi { get; set; }
    }
}
