namespace CRMService.Application.Models.ConfigClass
{
    public class AuthorizationOptions
    {
        public const string SectionName = "JWTSymmetricSecurityKey";
        public string JWTSymmetricSecurityKey { get; set; } = string.Empty;
    }
}



