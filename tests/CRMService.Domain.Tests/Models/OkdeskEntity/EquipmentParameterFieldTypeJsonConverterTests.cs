using System.Text.Json;
using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Domain.Tests.Models.OkdeskEntity;

public class EquipmentParameterFieldTypeJsonConverterTests
{
    public static TheoryData<string, EquipmentParameterFieldType> ValidValues => new()
    {
        { "0", EquipmentParameterFieldType.String },
        { "1", EquipmentParameterFieldType.Date },
        { "2", EquipmentParameterFieldType.DateTime },
        { "3", EquipmentParameterFieldType.Checkbox },
        { "4", EquipmentParameterFieldType.SingleSelect },
        { "5", EquipmentParameterFieldType.MultiSelect },
        { "\"ftstring\"", EquipmentParameterFieldType.String },
        { "\"ftdate\"", EquipmentParameterFieldType.Date },
        { "\"ftdatetime\"", EquipmentParameterFieldType.DateTime },
        { "\"ftcheckbox\"", EquipmentParameterFieldType.Checkbox },
        { "\"ftselect\"", EquipmentParameterFieldType.SingleSelect },
        { "\"ftmultiselect\"", EquipmentParameterFieldType.MultiSelect }
    };

    [Theory]
    [MemberData(nameof(ValidValues))]
    public void Deserialize_ValidValue_ReturnsExpectedType(
        string json,
        EquipmentParameterFieldType expected)
    {
        EquipmentParameterFieldType? actual = Deserialize(json);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("99")]
    [InlineData("\"unknown\"")]
    public void Deserialize_UnknownValue_ReturnsNull(string json)
    {
        Assert.Null(Deserialize(json));
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        Assert.Null(Deserialize("null"));
    }

    [Fact]
    public void Deserialize_UnsupportedToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => Deserialize("true"));
    }

    [Theory]
    [InlineData(EquipmentParameterFieldType.String)]
    [InlineData(EquipmentParameterFieldType.Date)]
    [InlineData(EquipmentParameterFieldType.DateTime)]
    [InlineData(EquipmentParameterFieldType.Checkbox)]
    [InlineData(EquipmentParameterFieldType.SingleSelect)]
    [InlineData(EquipmentParameterFieldType.MultiSelect)]
    public void SerializeThenDeserialize_DefinedValue_RoundTrips(EquipmentParameterFieldType expected)
    {
        JsonSerializerOptions options = CreateOptions();

        string json = JsonSerializer.Serialize<EquipmentParameterFieldType?>(expected, options);
        EquipmentParameterFieldType? actual =
            JsonSerializer.Deserialize<EquipmentParameterFieldType?>(json, options);

        Assert.Equal(((int)expected).ToString(), json);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SerializeThenDeserialize_Null_RoundTrips()
    {
        JsonSerializerOptions options = CreateOptions();

        string json = JsonSerializer.Serialize<EquipmentParameterFieldType?>(null, options);
        EquipmentParameterFieldType? actual =
            JsonSerializer.Deserialize<EquipmentParameterFieldType?>(json, options);

        Assert.Equal("null", json);
        Assert.Null(actual);
    }

    private static EquipmentParameterFieldType? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<EquipmentParameterFieldType?>(json, CreateOptions());
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new EquipmentParameterFieldTypeJsonConverter());
        return options;
    }
}
