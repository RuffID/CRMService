using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class Role : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public void CopyData(Role newItem)
        {
            Name = newItem.Name;
        }
    }
}