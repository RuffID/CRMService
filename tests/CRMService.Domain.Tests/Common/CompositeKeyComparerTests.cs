using CRMService.Domain.Common;
using Xunit;

namespace CRMService.Domain.Tests.Common;

public class CompositeKeyComparerTests
{
    private static readonly IEqualityComparer<TestItem> Comparer =
        CompositeKeyComparer.For<TestItem, int, string>(item => item.Number, item => item.Code);

    [Fact]
    public void Equals_EqualComponents_ReturnsTrue()
    {
        TestItem first = new(10, "code");
        TestItem second = new(10, "code");

        Assert.True(Comparer.Equals(first, second));
    }

    [Theory]
    [InlineData(11, "code")]
    [InlineData(10, "other")]
    public void Equals_OneComponentDiffers_ReturnsFalse(int number, string code)
    {
        TestItem first = new(10, "code");
        TestItem second = new(number, code);

        Assert.False(Comparer.Equals(first, second));
    }

    [Fact]
    public void Equals_ComponentsAreReversed_ReturnsFalse()
    {
        IEqualityComparer<TestItem> comparer =
            CompositeKeyComparer.For<TestItem, string, string>(item => item.First, item => item.Second);

        Assert.False(comparer.Equals(new TestItem("left", "right"), new TestItem("right", "left")));
    }

    [Fact]
    public void Equals_BothValuesAreNull_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_OnlyOneValueIsNull_ReturnsFalse()
    {
        TestItem item = new(10, "code");

        Assert.False(Comparer.Equals(item, null));
        Assert.False(Comparer.Equals(null, item));
    }

    [Fact]
    public void GetHashCode_EqualValues_ReturnsSameHashCode()
    {
        TestItem first = new(10, "code");
        TestItem second = new(10, "code");

        Assert.True(Comparer.Equals(first, second));
        Assert.Equal(Comparer.GetHashCode(first), Comparer.GetHashCode(second));
    }

    private class TestItem
    {
        public TestItem(int number, string code)
        {
            Number = number;
            Code = code;
            First = string.Empty;
            Second = string.Empty;
        }

        public TestItem(string first, string second)
        {
            Number = 0;
            Code = string.Empty;
            First = first;
            Second = second;
        }

        public int Number { get; }

        public string Code { get; }

        public string First { get; }

        public string Second { get; }
    }
}
