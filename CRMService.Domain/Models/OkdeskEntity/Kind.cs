using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class Kind : IEntity<int>, ICopyable<Kind>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool? Visible { get; set; }

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

        public virtual ICollection<KindParam> KindParams { get; set; } = new List<KindParam>();

        public virtual ICollection<Model> Models { get; set; } = new List<Model>();

        public void CopyData(Kind kind)
        {
            Code = kind.Code;
            Name = kind.Name;
            Description = kind.Description;
            Visible = kind.Visible;
        }
    }
}


