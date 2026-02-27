using EFCoreLibrary.Abstractions.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.OkdeskEntity
{
    public class KindsParameter : IEntity<int>, ICopyable<KindsParameter>
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        [JsonProperty("field_type")]
        public string? FieldType { get; set; }

        [NotMapped]
        public string[]? Equipment_kind_codes { get; set; }

        public virtual ICollection<KindParam> KindParams { get; set; } = new List<KindParam>();

        public virtual ICollection<EquipmentParameter> Parameters { get; set; } = new List<EquipmentParameter>();

        public void CopyData(KindsParameter parameter)
        {
            Code = parameter.Code;
            Name = parameter.Name;
            FieldType = parameter.FieldType;
            Equipment_kind_codes = parameter.Equipment_kind_codes;
        }
    }
}