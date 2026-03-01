namespace CRMService.Contracts.Models.Request
{
    public class SetUserActiveRequest
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
    }
}



