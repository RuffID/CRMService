namespace CRMService.Models.Authorization
{
    public class RefreshModel(string refreshToken)
    {
        public string RefreshToken { get; set; } = refreshToken;
    }
}
