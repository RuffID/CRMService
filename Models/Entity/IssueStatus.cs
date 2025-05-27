using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class IssueStatus : IEntity
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

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