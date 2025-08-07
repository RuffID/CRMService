using AutoMapper;
using CRMService.Dto.Authorization;
using CRMService.Interfaces.Repository;
using CRMService.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers.Authorization
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlockingController(IMapper mapper, IUnitOfWorkAuthorization unitOfWork) : Controller
    {
        [HttpGet("list"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> GetBlocks([FromQuery] int startIndex = 0, [FromQuery] int endIndex = 100)
        {
            IEnumerable<BlockReasonDto>? blocks = mapper.Map<IEnumerable<BlockReasonDto>>(await unitOfWork.BlockReason.GetAllItem(new Range(startIndex, endIndex)));

            if (blocks == null || !blocks.Any())
                return NotFound();

            return Ok(blocks);
        }

        [HttpPut("block"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> BlockUser([FromQuery] Guid userId, string reason_for_blocking)
        {
            BlockReason block = new()
            {
                UserId = userId,
                BlockingDate = DateTime.Now,
                ReasonBlock = reason_for_blocking
            };

            if (await unitOfWork.BlockReason.GetBlockByUserAndDate(block) != null)
                return BadRequest($"The user has already been blocked.");

            User? user = await unitOfWork.User.GetItem(new User() { Id = userId });
            if (user == null)
                return BadRequest("The user with the specified ID was not found.");

            if (user.Active == false)
                return BadRequest("User already is deactivated.");

            user.Active = false;
            unitOfWork.User.Update(user);
            await unitOfWork.SaveAsync();

            block.Id = Guid.NewGuid();
            unitOfWork.BlockReason.Create(block);
            await unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("unblock"), Authorize(Roles = RolesDefinition.ADMIN)]
        public async Task<IActionResult> UnblockUser([FromQuery] Guid userId, string reason_for_unblocking)
        {
            BlockReason? block = await unitOfWork.BlockReason.GetLastBlockOfUser(new BlockReason() { UserId = userId });

            if (block == null)
                return BadRequest($"No user block found.");

            User? user = await unitOfWork.User.GetItem(new User() { Id = userId });
            if (user == null)
                return BadRequest("The user with the specified ID was not found.");

            if (user.Active == true)
                return BadRequest("The user is not blocked.");

            user.Active = true;
            unitOfWork.User.Update(user);
            await unitOfWork.SaveAsync();

            block.UnblockingDate = DateTime.Now;
            block.UnblockingReason = reason_for_unblocking;
            unitOfWork.BlockReason.Update(block);
            await unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
