namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class EquipmentListPageDto
    {
        public List<EquipmentListItemDto> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int DisplayTotalCount { get; set; }

        public bool IsTotalCountCapped { get; set; }

        public bool HasNextPage { get; set; }
    }
}
