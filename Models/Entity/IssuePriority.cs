using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class IssuePriority : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public int? Position { get; set; }

        public string? Color { get; set; }

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public void CopyData(IssuePriority priority)
        {
            Name = priority.Name;
            Code = priority.Code;
            Position = priority.Position;
            Color = priority.Color;
        }
    }
}