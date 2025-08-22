namespace CRMService.Models.ConfigClass
{
    public class BearerToken
    {
        public const string BEARER_TOKEN = "BearerToken";
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? JWTKey { get; set; }
    }
}
