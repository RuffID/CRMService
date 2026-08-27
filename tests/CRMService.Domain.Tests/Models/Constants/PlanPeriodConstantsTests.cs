using CRMService.Domain.Models.Constants;
using Xunit;

namespace CRMService.Domain.Tests.Models.Constants;

public class PlanPeriodConstantsTests
{
    [Theory]
    [InlineData("day")]
    [InlineData("DAY")]
    [InlineData("Week")]
    [InlineData("MONTH")]
    [InlineData("year")]
    public void All_KnownPeriodWithAnyCasing_ContainsValue(string period)
    {
        Assert.Contains(period, PlanPeriodConstants.All);
    }

    [Fact]
    public void All_UnknownPeriod_DoesNotContainValue()
    {
        Assert.DoesNotContain("quarter", PlanPeriodConstants.All);
    }
}
