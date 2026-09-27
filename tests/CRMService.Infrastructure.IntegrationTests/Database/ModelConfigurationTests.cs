using CRMService.Application.Models.OkdeskSource;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

public class ModelConfigurationTests
{
    [Fact]
    public void MainContext_KeyTablesLengthsRelationshipsAndConverter_MatchContract()
    {
        DbContextOptions<MainContext> options = new DbContextOptionsBuilder<MainContext>()
            .UseSqlServer("Server=localhost;Database=metadata_only;User Id=sa;Password=Unused_Strong!1;TrustServerCertificate=True")
            .Options;
        using MainContext context = new(options);

        IEntityType user = context.Model.FindEntityType(typeof(User))!;
        Assert.Equal("User", user.GetTableName());
        Assert.Equal(45, user.FindProperty(nameof(User.Login))!.GetMaxLength());
        Assert.False(user.FindProperty(nameof(User.Login))!.IsNullable);
        Assert.True(user.FindProperty(nameof(User.EmployeeId))!.IsNullable);
        Assert.Equal(DeleteBehavior.SetNull, user.GetForeignKeys().Single(x => x.Properties.Single().Name == nameof(User.EmployeeId)).DeleteBehavior);

        IEntityType parameter = context.Model.FindEntityType(typeof(EquipmentParameter))!;
        Assert.Equal([nameof(EquipmentParameter.EquipmentId), nameof(EquipmentParameter.KindParameterId)], parameter.FindPrimaryKey()!.Properties.Select(x => x.Name));
        Assert.NotNull(parameter.FindProperty(nameof(EquipmentParameter.Value))!.GetValueConverter());

        IEntityType kindParameter = context.Model.FindEntityType(typeof(KindsParameter))!;
        Assert.Equal(ValueGenerated.OnAdd, kindParameter.FindProperty(nameof(KindsParameter.Id))!.ValueGenerated);
        Assert.Equal("NEXT VALUE FOR [KindsParameterLocalIdSequence]", kindParameter.FindProperty(nameof(KindsParameter.Id))!.GetDefaultValueSql());
        Assert.True(kindParameter.GetIndexes().Single(index => index.Properties.Single().Name == nameof(KindsParameter.Code)).IsUnique);
        Assert.True(kindParameter.GetIndexes().Single(index => index.Properties.Single().Name == nameof(KindsParameter.OkdeskId)).IsUnique);
        Assert.Equal(-1, context.Model.FindSequence("KindsParameterLocalIdSequence")!.StartValue);
        Assert.Equal(-1, context.Model.FindSequence("KindsParameterLocalIdSequence")!.IncrementBy);

        IEntityType issue = context.Model.FindEntityType(typeof(Issue))!;
        Assert.Equal("Issue", issue.GetTableName());
        Assert.Equal(3000, issue.FindProperty(nameof(Issue.Title))!.GetMaxLength());
        Assert.True(issue.FindProperty(nameof(Issue.GroupId))!.IsNullable);
        Assert.Contains(
            issue.GetIndexes(),
            index => index.GetDatabaseName() == "issue_groupId_idx"
                && index.Properties.Single().Name == nameof(Issue.GroupId));
        Assert.True(issue.FindProperty(nameof(Issue.StatusId))!.IsNullable);
        Assert.All(issue.GetForeignKeys(), key => Assert.Equal(DeleteBehavior.Restrict, key.DeleteBehavior));
    }

    [Fact]
    public void OkdeskContext_TableKeysShadowColumnsAndDeleteBehavior_MatchCloudSchema()
    {
        DbContextOptions<OkdeskContext> options = new DbContextOptionsBuilder<OkdeskContext>()
            .UseSqlServer("Server=localhost;Database=metadata_only;User Id=sa;Password=Unused_Strong!1;TrustServerCertificate=True")
            .Options;
        using OkdeskContext context = new(options);

        IEntityType company = context.Model.FindEntityType(typeof(Company))!;
        Assert.Equal("companies", company.GetTableName());
        Assert.Equal("sequential_id", company.FindProperty(nameof(Company.Id))!.GetColumnName());
        Assert.Equal("id", company.FindProperty("InternalId")!.GetColumnName());
        Assert.Single(company.GetKeys(), x => !x.IsPrimaryKey());
        Assert.Equal(DeleteBehavior.NoAction, company.GetForeignKeys().Single().DeleteBehavior);

        IEntityType equipment = context.Model.FindEntityType(typeof(Equipment))!;
        Assert.Equal("equipments", equipment.GetTableName());
        Assert.NotNull(equipment.FindProperty("ParametersJson"));
        Assert.Null(equipment.FindProperty(nameof(Equipment.Parameters)));

        IEntityType employee = context.Model.FindEntityType(typeof(Employee))!;
        Assert.Equal("users", employee.GetTableName());
        Assert.Equal("type", employee.FindProperty("Type")!.GetColumnName());

        IEntityType issue = context.Model.FindEntityType(typeof(Issue))!;
        Assert.Equal("issues", issue.GetTableName());
        Assert.Null(issue.FindProperty(nameof(Issue.GroupId)));
        Assert.Equal("group_id", issue.FindProperty("GroupInternalId")!.GetColumnName());

        IEntityType group = context.Model.FindEntityType(typeof(Group))!;
        Assert.Equal("groups", group.GetTableName());
        Assert.Equal("sequential_id", group.FindProperty(nameof(Group.Id))!.GetColumnName());
        Assert.Equal("id", group.FindProperty("InternalId")!.GetColumnName());

        IEntityType kindParameter = context.Model.FindEntityType(typeof(OkdeskKindParameterRecord))!;
        Assert.Equal("equipment_parameters", kindParameter.GetTableName());
        Assert.Equal("id", kindParameter.FindProperty(nameof(OkdeskKindParameterRecord.Id))!.GetColumnName());

        IEntityType kindParameterConnection = context.Model.FindEntityType(typeof(OkdeskKindParameterConnectionRecord))!;
        Assert.Equal("equipment_kind_parameters", kindParameterConnection.GetTableName());
        Assert.Equal("parameter_id", kindParameterConnection.FindProperty(nameof(OkdeskKindParameterConnectionRecord.KindParameterId))!.GetColumnName());
    }
}
