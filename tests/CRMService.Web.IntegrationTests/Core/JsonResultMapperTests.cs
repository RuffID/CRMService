using System.Text.Json;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Web.Core.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Web.IntegrationTests.Core;

public class JsonResultMapperTests
{
    [Fact]
    public void ToJsonResult_SuccessWithoutData_ReturnsSuccessEnvelope()
    {
        JsonResult result = JsonResultMapper.ToJsonResult(ServiceResult.Ok());

        using JsonDocument json = SerializeValue(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public void ToJsonResult_GenericSuccess_ReturnsSingleSuccessEnvelopeWithData()
    {
        JsonResult result = JsonResultMapper.ToJsonResult(ServiceResult<string>.Ok("payload"));

        using JsonDocument json = SerializeValue(result);
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("payload", data.GetString());
        Assert.Equal(JsonValueKind.String, data.ValueKind);
    }

    [Fact]
    public void ToJsonResult_ExpectedFailure_ReturnsStatusAndFailureEnvelope()
    {
        JsonResult result = JsonResultMapper.ToJsonResult(
            ServiceResult<int>.Fail(StatusCodes.Status422UnprocessableEntity, "Validation failed."));

        using JsonDocument json = SerializeValue(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Validation failed.", json.RootElement.GetProperty("message").GetString());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    private static JsonDocument SerializeValue(JsonResult result) =>
        JsonDocument.Parse(JsonSerializer.Serialize(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
}
