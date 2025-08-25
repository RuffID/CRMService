using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class OkdeskRole : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public void CopyData(OkdeskRole newItem)
        {
            Name = newItem.Name;
        }
    }
}