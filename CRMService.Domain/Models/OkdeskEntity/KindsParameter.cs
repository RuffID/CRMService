using EFCoreLibrary.Abstractions.Entity;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class KindsParameter : IEntity<int>, ICopyable<KindsParameter>
    {
        public int Id { get; set; }

        public int? OkdeskId { get; private set; }

        public string Code { get; private set; } = string.Empty;

        public string? Name { get; private set; }

        public EquipmentParameterFieldType? FieldType { get; private set; }

        public virtual ICollection<KindParam> KindParams { get; set; } = new List<KindParam>();

        public virtual ICollection<EquipmentParameter> Parameters { get; set; } = new List<EquipmentParameter>();

        private KindsParameter()
        {
        }

        public KindsParameter(string code, string? name, EquipmentParameterFieldType? fieldType, int? okdeskId = null)
        {
            UpdateDetails(code, name, fieldType);

            if (okdeskId.HasValue)
                SetOkdeskId(okdeskId.Value);
        }

        public void CopyData(KindsParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);

            UpdateDetails(parameter.Code, parameter.Name, parameter.FieldType);

            if (parameter.OkdeskId.HasValue)
                SetOkdeskId(parameter.OkdeskId.Value);
        }

        public void UpdateDetails(string code, string? name, EquipmentParameterFieldType? fieldType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);

            Code = code;
            Name = name;
            FieldType = fieldType;
        }

        public void SetOkdeskId(int okdeskId)
        {
            if (okdeskId <= 0)
                throw new ArgumentOutOfRangeException(nameof(okdeskId), okdeskId, "Okdesk identifier must be positive.");

            if (OkdeskId.HasValue && OkdeskId.Value != okdeskId)
                throw new InvalidOperationException($"Okdesk identifier for kind parameter '{Code}' cannot be changed from '{OkdeskId.Value}' to '{okdeskId}'.");

            OkdeskId = okdeskId;
        }
    }
}
