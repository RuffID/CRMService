using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class IssueStatus : IEntity<int>, ICopyable<IssueStatus>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public void CopyData(IssueStatus status)
        {
            Code = status.Code;
            Name = status.Name;
            Color = status.Color;
        }
    }
}