using CRMService.Application.Service.Authorization;
using CRMService.Domain.Models.Authorization;
using Xunit;

namespace CRMService.Application.Tests.Service.Authorization;

public class GenerateRandomStringTests
{
    private const string ALLOWED_CHARACTERS =
        ConstSymbols.UPALPHABET + ConstSymbols.LOWALPHABET + ConstSymbols.NUMBERS + ConstSymbols.SYMBOLS;

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(128)]
    public void GetRandomString_PositiveLength_ReturnsRequestedLength(int length)
    {
        GenerateRandomString generator = new();

        Assert.Equal(length, generator.GetRandomString(length).Length);
    }

    [Fact]
    public void GetRandomString_Result_ContainsOnlyDeclaredCharacters()
    {
        GenerateRandomString generator = new();

        string value = generator.GetRandomString(512);

        Assert.All(value, character => Assert.Contains(character, ALLOWED_CHARACTERS));
    }

    [Fact]
    public void GetRandomString_RepeatedCalls_ProduceMultipleValues()
    {
        GenerateRandomString generator = new();

        HashSet<string> values = Enumerable.Range(0, 20)
            .Select(_ => generator.GetRandomString(32))
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(values.Count > 1);
    }

    [Fact]
    public void GetRandomString_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        GenerateRandomString generator = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GetRandomString(-1));
    }

    [Fact]
    public void GetRandomString_ZeroLength_ReturnsEmptyString()
    {
        GenerateRandomString generator = new();

        Assert.Equal(string.Empty, generator.GetRandomString(0));
    }

    [Fact]
    public void GetBase64RandomString_Result_DecodesToExpectedByteCount()
    {
        GenerateRandomString generator = new();

        byte[] bytes = Convert.FromBase64String(generator.GetBase64RandomString());

        Assert.Equal(64, bytes.Length);
    }
}
