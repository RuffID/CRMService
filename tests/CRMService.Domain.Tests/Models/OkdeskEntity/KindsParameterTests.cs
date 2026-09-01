using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Domain.Tests.Models.OkdeskEntity;

public class KindsParameterTests
{
    [Fact]
    public void SetOkdeskId_FirstAssignment_Succeeds()
    {
        KindsParameter parameter = new("ESM", "Лицензия ЕСМ до", EquipmentParameterFieldType.Date);

        parameter.SetOkdeskId(10503);

        Assert.Equal(10503, parameter.OkdeskId);
    }

    [Fact]
    public void SetOkdeskId_DifferentSecondAssignment_Throws()
    {
        KindsParameter parameter = new("ESM", null, null, 10503);

        Assert.Throws<InvalidOperationException>(() => parameter.SetOkdeskId(10504));
        Assert.Equal(10503, parameter.OkdeskId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetOkdeskId_NonPositiveValue_Throws(int okdeskId)
    {
        KindsParameter parameter = new("ESM", null, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => parameter.SetOkdeskId(okdeskId));
    }
}
