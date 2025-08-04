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

        public void CopyData(IssueType type)
        {
            Code = type.Code;
            Name = type.Name;
            Default = type.Default;
            Inner = type.Inner;
            AvailableForClient = type.AvailableForClient;
            Type = type.Type;
        }
    }
}