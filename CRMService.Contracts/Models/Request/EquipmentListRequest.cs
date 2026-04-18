namespace CRMService.Contracts.Models.Request
{
    public class EquipmentListRequest
    {
        public int? EquipmentId { get; set; }

        public List<int>? CompanyIds { get; set; }

        public List<int>? MaintenanceEntityIds { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
