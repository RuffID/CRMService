using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Tests.Builders;

public class EquipmentBuilder
{
    private int id = 1;
    private string inventoryNumber = "INV-1";

    public EquipmentBuilder WithId(int value)
    {
        id = value;
        return this;
    }

    public Equipment Build()
    {
        return new Equipment
        {
            Id = id,
            InventoryNumber = inventoryNumber
        };
    }
}
