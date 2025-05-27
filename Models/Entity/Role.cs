using CRMService.Interfaces.Entity;
using Newtonsoft.Json;

namespace CRMService.Models.Entity
{
    public class Role : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}