using CRMService.Application.Common.Exceptions;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Web.Core.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Web.IntegrationTests.TestEndpoints;

[ApiController]
[Route("api/test-probe")]
public class TestProbeController : ControllerBase
{
    [Authorize]
    [HttpGet("authorized")]
    public IActionResult AuthorizedEndpoint() => Ok(new { success = true });

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult AdminEndpoint() => Ok(new { success = true });

    [HttpGet("expected-failure")]
    public IActionResult ExpectedFailure() =>
        JsonResultMapper.ToJsonResult(ServiceResult.Fail(StatusCodes.Status409Conflict, "Expected conflict."));

    [HttpGet("infrastructure-error")]
    public IActionResult InfrastructureError() =>
        throw new ExternalServiceException("secret-value from upstream");

    [HttpGet("unknown-error")]
    public IActionResult UnknownError() =>
        throw new InvalidOperationException("secret-value from application");

    [HttpGet("cancelled")]
    public IActionResult Cancelled() => throw new OperationCanceledException();
}
