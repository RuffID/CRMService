namespace CRMService.Contracts.Models.Request
{
    public class EquipmentLookupListRequest : LookupListRequest
    {
        public List<int>? TypeIds { get; set; }

        public List<int>? ManufacturerIds { get; set; }

        public List<int>? ModelIds { get; set; }

        public List<int>? CompanyIds { get; set; }
    }
}
