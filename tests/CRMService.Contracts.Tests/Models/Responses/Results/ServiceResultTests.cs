using CRMService.Contracts.Models.Responses.Results;
using Xunit;

namespace CRMService.Contracts.Tests.Models.Responses.Results;

public class ServiceResultTests
{
    [Fact]
    public void Ok_WithoutData_ReturnsSuccessfulResultWithoutError()
    {
        ServiceResult result = ServiceResult.Ok();

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_WithoutData_ReturnsErrorWithStatusCodeAndMessage()
    {
        ServiceResult result = ServiceResult.Fail(409, "Conflict");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(409, result.Error.StatusCode);
        Assert.Equal("Conflict", result.Error.Message);
    }

    [Fact]
    public void Ok_WithData_ReturnsSuccessfulResultWithSameDataAndWithoutError()
    {
        TestData data = new() { Value = "payload" };

        ServiceResult<TestData> result = ServiceResult<TestData>.Ok(data);

        Assert.True(result.Success);
        Assert.Same(data, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_WithDataType_ReturnsDefaultDataAndError()
    {
        ServiceResult<int> result = ServiceResult<int>.Fail(404, "Not found");

        Assert.False(result.Success);
        Assert.Equal(default, result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal("Not found", result.Error.Message);
    }

    private class TestData
    {
        public string Value { get; init; } = string.Empty;
    }
}
