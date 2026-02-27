using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class IssuePriority : IEntity<int>, ICopyable<IssuePriority>
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string Code { get; set; } = string.Empty;

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