/*using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using CRMService.Models.Constants;
using CRMService.Models.Dto.Mappers.Authorize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/authorize/[controller]")]
    public class BlockingController(IUnitOfWork unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> GetBlocks([FromQuery] int skip = 0, [FromQuery] int limit = LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_DB, CancellationToken ct = default)
        {
            List<BlockReason> blocks = await unitOfWork.BlockReason.GetItemsByPredicate(skip: skip, take: limit, asNoTracking: true, ct: ct);

            return Ok(blocks.ToDto());
        }

        [HttpPut("block"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> BlockUser([FromQuery] Guid userId, string reasonForBlocking, CancellationToken ct)
        {
            BlockReason block = new()
            {
                UserId = userId,
                BlockingDate = DateTime.Now,
                ReasonBlock = reasonForBlocking
            }; 

            BlockReason? existBlock = await unitOfWork.BlockReason.GetItemByPredicate(b => b.UserId == userId && !b.UnblockingDate.HasValue, asNoTracking: true, ct: ct);

            if (existBlock != null)
                return Conflict($"User has already been blocked.");

            User? user = await unitOfWork.User.GetItemById(userId, true, ct);
            if (user == null)
                return NotFound($"User {userId} not found.");

            if (!user.Active)
                return BadRequest($"User {userId} is already blocked.");

            user.Active = false;

            unitOfWork.BlockReason.Create(block);
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }

        [HttpPut("unblock"), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<IActionResult> UnblockUser([FromQuery] Guid userId, string reason_for_unblocking, CancellationToken ct)
        {
            BlockReason? block = await unitOfWork.BlockReason.GetItemByPredicate(predicate: b => b.UserId == userId && b.UnblockingDate == null, asNoTracking: false, ct);

            if (block == null)
                return NotFound($"No blocks found for user {userId}.");

            User? user = await unitOfWork.User.GetItemById(userId, asNoTracking: false, ct);

            if (user == null)
                return NotFound($"User {userId} not found.");

            if (user.Active == true)
                return BadRequest($"User {userId} is not blocked.");

            user.Active = true;
            block.UnblockingDate = DateTime.Now;
            block.UnblockingReason = reason_for_unblocking;
            await unitOfWork.SaveAsync(ct);

            return NoContent();
        }
    }
}
*/