using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class IssueType : IEntity<int>, ICopyable<IssueType>
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
            GroupId = type.GroupId;
            AvailableForClient = type.AvailableForClient;
        }
    }
}