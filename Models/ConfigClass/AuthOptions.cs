namespace CRMService.Models.ConfigClass
{
    public class AuthOptions
    {
        public const string AUTHORIZATION_OPTIONS = "AuthorizationOptions";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SymmetricSecurityKey { get; set; } = string.Empty;
        public int AccessTokenLifeTimeFromMinutes {  get; set; }
        public int RefreshTokenLifeTimeFromDays { get; set; }
    }
}
