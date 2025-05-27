using CRMService.Interfaces.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.Entity
{
    public class IssueType : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public bool? Default { get; set; }

        public bool? Inner { get; set; }

        [JsonProperty("available_for_client")]
        public bool? AvailableForClient { get; set; }

        public string? Type { get; set; }

        [NotMapped]
        public List<IssueType>? Children { get; set; }

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public void CopyData(IssueType status)
        {
            Code = status.Code;
            Name = status.Name;
            Default = status.Default;
            Inner = status.Inner;
            AvailableForClient = status.AvailableForClient;
            Type = status.Type;
        }
    }
}