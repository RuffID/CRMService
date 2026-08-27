using CRMService.Application.Abstractions.Database.Repository;
using Xunit;

namespace CRMService.Application.Tests.Database
{
    public class ScenarioUnitOfWorkContractTests
    {
        [Theory]
        [InlineData(typeof(IAuthorizationUnitOfWork), "CrmRole,Employee,Session,User")]
        [InlineData(typeof(IPlanSettingsUnitOfWork), "GeneralSettings,Plan,PlanColor,PlanSetting")]
        [InlineData(typeof(IReportsUnitOfWork), "Employee,EmployeeGroup,EmployeePerformanceReport,Group,IssueDynamicsChartReport,Plan,PlanSetting,SpentTimeChartReport")]
        [InlineData(typeof(ICompanyDirectoryUnitOfWork), "Company,CompanyCategory,Employee,EmployeeGroup,EmployeeRole,Group,OkdeskRole")]
        [InlineData(typeof(IEquipmentUnitOfWork), "Company,Equipment,Kind,KindParameter,KindParams,MaintenanceEntity,Manufacturer,Model,Parameter")]
        [InlineData(typeof(IIssuesUnitOfWork), "Company,Employee,Issue,IssuePriority,IssueStatus,IssueType,IssueTypeGroup,MaintenanceEntity,TimeEntry")]
        public void ScenarioContract_ActualConsumerBoundary_ExposesOnlyExpectedRepositories(
            Type contractType,
            string expectedPropertyNames)
        {
            string[] expected = expectedPropertyNames.Split(',').OrderBy(name => name).ToArray();
            string[] actual = contractType
                .GetProperties()
                .Where(property => property.DeclaringType == contractType)
                .Select(property => property.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.True(typeof(IUnitOfWorkScope).IsAssignableFrom(contractType));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(typeof(IOkdeskCompanyDirectorySource), "Company,CompanyCategory,Employee,Group")]
        [InlineData(typeof(IOkdeskEquipmentSource), "Equipment,Kind,KindParameter,KindParams,MaintenanceEntity,Manufacturer,Model")]
        [InlineData(typeof(IOkdeskIssuesSource), "Employee,Issue,IssuePriority,IssueStatus,IssueType,IssueTypeGroup,TimeEntry")]
        public void OkdeskSource_ActualConsumerBoundary_ExposesOnlyExpectedRepositories(
            Type contractType,
            string expectedPropertyNames)
        {
            string[] expected = expectedPropertyNames.Split(',').OrderBy(name => name).ToArray();
            string[] actual = contractType.GetProperties().Select(property => property.Name).OrderBy(name => name).ToArray();

            Assert.False(typeof(IUnitOfWorkScope).IsAssignableFrom(contractType));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ApplicationAssembly_GlobalUnitOfWorkContracts_AreRemoved()
        {
            Type[] types = typeof(IUnitOfWorkScope).Assembly.GetTypes();

            Assert.DoesNotContain(types, type => type.Name is "IUnitOfWork" or "IOkdeskUnitOfWork");
        }
    }
}
