namespace CRMService.Models.Entity.Results
{
    public sealed class IssueTypeCombinedResult
    {
        public List<IssueTypeGroup> Groups { get; } = new List<IssueTypeGroup>();
        public List<IssueType> Types { get; } = new List<IssueType>();
    }
}
