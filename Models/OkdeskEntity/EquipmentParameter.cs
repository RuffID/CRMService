using CRMService.Interfaces.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class EquipmentParameter : ICopyable<EquipmentParameter>
    {
        public int EquipmentId { get; set; }

        public int KindParameterId { get; set; }

        public string Value { get; set; } = string.Empty;

        public virtual Equipment Equipment { get; set; } = null!;

        public virtual KindsParameter KindParameter { get; set; } = null!;

        public void CopyData(EquipmentParameter parameter)
        {
            Value = parameter.Value;
        }
    }
}