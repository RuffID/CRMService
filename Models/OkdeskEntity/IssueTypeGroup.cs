using CRMService.Interfaces.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class IssueTypeGroup : IEntity<int>, ICopyable<IssueTypeGroup>
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }

        public virtual IssueTypeGroup? Parent { get; set; }
        public virtual ICollection<IssueTypeGroup> Children { get; set; } = new List<IssueTypeGroup>();
        public virtual ICollection<IssueType> Types { get; set; } = new List<IssueType>();

        public void CopyData(IssueTypeGroup entity)
        {
            Code = entity.Code;
            Name = entity.Name;
            ParentGroupId = entity.ParentGroupId;
        }
    }
}
