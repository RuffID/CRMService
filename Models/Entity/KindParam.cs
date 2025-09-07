namespace CRMService.Models.Entity
{
    public class KindParam
    {
        public int KindId { get; set; }

        public int KindParameterId { get; set; }

        public virtual Kind Kind { get; set; } = null!;

        public virtual KindsParameter KindParameter { get; set; } = null!;
    }
}