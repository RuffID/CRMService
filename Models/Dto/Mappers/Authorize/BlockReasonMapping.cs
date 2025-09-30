using CRMService.Models.Authorization;
using CRMService.Models.Dto.Authorization;

namespace CRMService.Models.Dto.Mappers.Authorize
{
    public static class BlockReasonMapping
    {
        public static IEnumerable<BlockReasonDto> ToDto(this IEnumerable<BlockReason> blocks)
        {
            foreach (BlockReason block in blocks)
                yield return block.ToDto();
        }

        public static BlockReasonDto ToDto(this BlockReason block)
        {
            return new BlockReasonDto()
            {
                Id = block.Id,
                UserId = block.UserId,
                BlockingDate = block.BlockingDate,
                UnblockingDate = block.UnblockingDate,
                ReasonBlock = block.ReasonBlock,
                UnblockingReason = block.UnblockingReason
            };
        }
    }
}
