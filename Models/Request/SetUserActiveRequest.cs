namespace CRMService.Models.Request
{
    public class SetUserActiveRequest
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
