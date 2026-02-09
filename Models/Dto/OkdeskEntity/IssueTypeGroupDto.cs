namespace CRMService.Models.Dto.OkdeskEntity
{
    public class IssueTypeGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
