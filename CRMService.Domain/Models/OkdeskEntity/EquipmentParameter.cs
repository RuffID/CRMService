using EFCoreLibrary.Abstractions.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class EquipmentParameter : ICopyable<EquipmentParameter>
    {
        public int EquipmentId { get; set; }

        [NotMapped]
        public string? Code { get; set; }

        public int? KindParameterId { get; set; }

        public object? Value { get; set; }

        public virtual Equipment? Equipment { get; set; }

        public virtual KindsParameter? KindParameter { get; set; }

        public void CopyData(EquipmentParameter parameter)
        {
            Value = parameter.Value;
        }
    }
}


