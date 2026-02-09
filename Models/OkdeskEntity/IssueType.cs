using CRMService.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class IssueType : IEntity<int>, IHasCode, ICopyable<IssueType>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public bool IsInner { get; set; }

        public bool AvailableForClient { get; set; }

        public int? GroupId { get; set; }

        public virtual IssueTypeGroup? Group { get; set; }

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public void CopyData(IssueType type)
        {
            Code = type.Code;
            Name = type.Name;
            IsDefault = type.IsDefault;
            IsInner = type.IsInner;
            AvailableForClient = type.AvailableForClient;
        }
    }
}