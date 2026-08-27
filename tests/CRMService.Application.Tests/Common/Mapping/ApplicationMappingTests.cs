using CRMService.Application.Common.Mapping.Authorize;
using CRMService.Application.Common.Mapping.OkdeskEntity;
using CRMService.Contracts.Models.Dto.Authorization;
using CRMService.Contracts.Models.Dto.OkdeskEntity;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using Xunit;

namespace CRMService.Application.Tests.Common.Mapping;

public class ApplicationMappingTests
{
    [Fact]
    public void ToDto_UserWithEmployeeAndRoles_PreservesIdentifiersAndDisplayData()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        User user = new()
        {
            Id = userId,
            Name = "CRM User",
            Login = "crm.user",
            Active = true,
            EmployeeId = 17,
            Employee = new Employee
            {
                Id = 17,
                LastName = "Иванов",
                FirstName = "Иван",
                Patronymic = "Иванович"
            },
            Roles = new List<CrmRole> { new() { Id = roleId, Name = "Admin" } }
        };

        UserDto dto = user.ToDto();

        Assert.Equal(userId, dto.Id);
        Assert.Equal("CRM User", dto.Name);
        Assert.Equal("crm.user", dto.Login);
        Assert.True(dto.Active);
        Assert.Equal(17, dto.EmployeeId);
        Assert.Equal("Иванов Иван Иванович", dto.EmployeeName);
        Assert.Collection(dto.Roles, role =>
        {
            Assert.Equal(roleId, role.Id);
            Assert.Equal("Admin", role.Name);
        });
    }

    [Fact]
    public void ToDto_UserWithoutEmployee_PreservesNullEmployeeContract()
    {
        User user = new() { EmployeeId = null, Employee = null };

        UserDto dto = user.ToDto();

        Assert.Null(dto.EmployeeId);
        Assert.Null(dto.EmployeeName);
    }

    [Fact]
    public void ToDto_Issue_PreservesExternalIdentifiersAndNullableDates()
    {
        Issue issue = new()
        {
            Id = 31,
            AssigneeId = 5,
            AuthorId = 6,
            Title = "Issue",
            StatusId = 7,
            PriorityId = 8,
            TypeId = 9,
            CompanyId = 10,
            ServiceObjectId = 11,
            CreatedAt = new DateTime(2026, 2, 1),
            CompletedAt = null
        };

        IssueDto dto = issue.ToDto();

        Assert.Equal(issue.Id, dto.Id);
        Assert.Equal(issue.AssigneeId, dto.AssigneeId);
        Assert.Equal(issue.AuthorId, dto.AuthorId);
        Assert.Equal(issue.StatusId, dto.StatusId);
        Assert.Equal(issue.PriorityId, dto.PriorityId);
        Assert.Equal(issue.TypeId, dto.TypeId);
        Assert.Equal(issue.CompanyId, dto.CompanyId);
        Assert.Equal(issue.ServiceObjectId, dto.ServiceObjectId);
        Assert.Null(dto.CompletedAt);
    }

    [Fact]
    public void ToDto_EquipmentWithMissingNavigationValues_LeavesNestedDtosNull()
    {
        Equipment equipment = new()
        {
            Id = 41,
            SerialNumber = "SERIAL",
            InventoryNumber = "INVENTORY",
            Parameters = new List<EquipmentParameter>
            {
                new() { Code = "ignored-code", Value = 123 }
            }
        };

        EquipmentDto dto = equipment.ToDto();

        Assert.Equal(41, dto.Id);
        Assert.Equal("SERIAL", dto.Serial_number);
        Assert.Equal("INVENTORY", dto.Inventory_number);
        Assert.Null(dto.Kind);
        Assert.Null(dto.Manufacturer);
        Assert.Null(dto.Model);
        Assert.Null(dto.Company);
        Assert.Collection(dto.Parameters!, parameter => Assert.Equal("123", parameter.Value));
    }

    [Fact]
    public void ToEntity_Model_TrimsDescriptionAndPreservesValidReferences()
    {
        ModelDto dto = new()
        {
            Id = 51,
            Name = "Model",
            Code = "model-code",
            Description = new string('x', 501),
            Visible = true,
            Kind = new KindDto { Id = 3 },
            Manufacturer = new ManufacturerDto { Id = 4 }
        };

        Model entity = dto.ToEntity();

        Assert.Equal(51, entity.Id);
        Assert.Equal("Model", entity.Name);
        Assert.Equal("model-code", entity.Code);
        Assert.Equal(500, entity.Description!.Length);
        Assert.True(entity.Visible);
        Assert.Equal(3, entity.KindId);
        Assert.Equal(4, entity.ManufacturerId);
    }

    [Fact]
    public void ToEntity_CompanyCategory_RoundTripsSignificantFields()
    {
        CompanyCategory source = new() { Id = 61, Name = "Client", Code = "client", Color = "#123456" };

        CompanyCategory result = source.ToDto().ToEntity();

        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Code, result.Code);
        Assert.Equal(source.Color, result.Color);
    }
}
