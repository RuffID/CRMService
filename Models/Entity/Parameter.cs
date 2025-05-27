using CRMService.Interfaces.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.Entity
{
    public class Parameter : IEntity
    {
        public int Id { get; set; }

        [NotMapped]
        public string? Code { get; set; }

        [NotMapped]
        public string? Name { get; set; }

        public int? EquipmentId { get; set; }

        public int? KindParameterId { get; set; }

        public object? Value { get; set; }

        public virtual Equipment? Equipment { get; set; }

        public virtual KindsParameter? KindParameter { get; set; }

        public void CopyData(Parameter parameter)
        {
            Code = parameter.Code;
            Name = parameter.Name;
            Value = parameter.Value;
            EquipmentId = parameter.EquipmentId;
            KindParameterId = parameter.KindParameterId;
            Value = parameter.Value;
        }
    }
}