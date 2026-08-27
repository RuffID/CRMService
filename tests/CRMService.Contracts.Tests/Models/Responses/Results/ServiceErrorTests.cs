using CRMService.Contracts.Models.Responses.Results;
using Xunit;

namespace CRMService.Contracts.Tests.Models.Responses.Results;

public class ServiceErrorTests
{
    [Fact]
    public void Constructor_StatusCodeAndMessage_PreservesValues()
    {
        ServiceError error = new(422, "Validation failed");

        Assert.Equal(422, error.StatusCode);
        Assert.Equal("Validation failed", error.Message);
    }
}
