using CRMService.Application.Service.Authorization;
using Xunit;

namespace CRMService.Application.Tests.Service.Authorization;

public class HasherTests
{
    private readonly Hasher hasher = new();

    [Fact]
    public void Verify_HashCreatedForSameInput_ReturnsTrue()
    {
        string hash = hasher.Hash("correct password");

        Assert.True(hasher.Verify("correct password", hash));
    }

    [Fact]
    public void Verify_DifferentInput_ReturnsFalse()
    {
        string hash = hasher.Hash("correct password");

        Assert.False(hasher.Verify("wrong password", hash));
    }

    [Fact]
    public void Hash_SameInputTwice_UsesDifferentSalts()
    {
        string first = hasher.Hash("password");
        string second = hasher.Hash("password");

        Assert.NotEqual(first, second);
        Assert.True(hasher.Verify("password", first));
        Assert.True(hasher.Verify("password", second));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("00.00.invalid.SHA256")]
    public void Verify_DamagedHash_Throws(string hash)
    {
        Assert.ThrowsAny<Exception>(() => hasher.Verify("password", hash));
    }

    [Fact]
    public void Hash_EmptyInput_CreatesVerifiableHash()
    {
        string hash = hasher.Hash(string.Empty);

        Assert.True(hasher.Verify(string.Empty, hash));
    }

    [Fact]
    public void Hash_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => hasher.Hash(null!));
    }
}
