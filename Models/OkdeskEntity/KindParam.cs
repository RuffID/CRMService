using CRMService.Core;

namespace CRMService.Models.OkdeskEntity
{
    public class KindParam
    {
        public int KindId { get; set; }

        public int KindParameterId { get; set; }

        public virtual Kind Kind { get; set; } = null!;

        public virtual KindsParameter KindParameter { get; set; } = null!;

        public static IEqualityComparer<KindParam> Comparer { get; } =
        CompositeKeyComparer.For<KindParam, int, int>(x => x.KindId, x => x.KindParameterId);
    }
}