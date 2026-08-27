using System.Text.Json;
using CRMService.Application.Abstractions.Service;
using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Authorization;
using CRMService.Application.Models.Report;
using CRMService.Application.Service.Authorization;
using CRMService.Contracts.Models.Dto.CrmEntities;
using CRMService.Contracts.Models.Request;
using CRMService.Contracts.Models.Responses.Results;
using CRMService.Domain.Models.Authorization;
using CRMService.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CRMService.Web.IntegrationTests.Pages;

public class RazorPageHandlerTests
{
    [Fact]
    public async Task Users_ListHandler_FiltersInactiveUsersAndMapsSingleEnvelope()
    {
        IUserRepository userRepository = Substitute.For<IUserRepository>();
        userRepository.GetAllWithRolesAndEmployeeReadOnlyAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User>
            {
                new() { Id = Guid.NewGuid(), Name = "Active", Login = "active", Active = true },
                new() { Id = Guid.NewGuid(), Name = "Inactive", Login = "inactive", Active = false }
            });
        IAuthorizationUnitOfWork unitOfWork = Substitute.For<IAuthorizationUnitOfWork>();
        unitOfWork.User.Returns(userRepository);
        UsersModel model = new(
            new UserService(unitOfWork, new Hasher()),
            new RoleService(unitOfWork),
            null!);

        JsonResult result = Assert.IsType<JsonResult>(
            await model.OnGetListAsync(includeInactive: false, TestContext.Current.CancellationToken));
        using JsonDocument json = SerializeValue(result);

        JsonElement data = json.RootElement.GetProperty("data");
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(1, data.GetArrayLength());
        Assert.Equal("active", data[0].GetProperty("login").GetString());
    }

    [Fact]
    public async Task Report_InvalidPeriod_ReturnsBadRequestWithoutCallingApplicationService()
    {
        IEmployeePerformanceReportService reportService = Substitute.For<IEmployeePerformanceReportService>();
        ReportModel model = CreateReportModel(reportService);
        ReportRequest request = new()
        {
            DateFrom = new DateTime(2026, 8, 28),
            DateTo = new DateTime(2026, 8, 27)
        };

        JsonResult result = Assert.IsType<JsonResult>(
            await model.OnPostReportAsync(request, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Empty(reportService.ReceivedCalls());
    }

    [Fact]
    public async Task Report_ValidPeriod_MapsApplicationDataOnce()
    {
        IEmployeePerformanceReportService reportService = Substitute.For<IEmployeePerformanceReportService>();
        reportService
            .GetFullReportOnEmployees(
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<ReportRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<ReportInfo>());
        ReportModel model = CreateReportModel(reportService);
        ReportRequest request = new()
        {
            DateFrom = new DateTime(2026, 8, 26),
            DateTo = new DateTime(2026, 8, 27)
        };

        JsonResult result = Assert.IsType<JsonResult>(
            await model.OnPostReportAsync(request, CancellationToken.None));
        using JsonDocument json = SerializeValue(result);

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Array, json.RootElement.GetProperty("data").ValueKind);
        Assert.Single(reportService.ReceivedCalls());
    }

    [Fact]
    public async Task PlanSettings_ApplicationFailure_PreservesStatusAndMessage()
    {
        IPlanSettingsService service = Substitute.For<IPlanSettingsService>();
        service.GetPlans(Arg.Any<CancellationToken>())
            .Returns(ServiceResult<List<PlanDto>>.Fail(StatusCodes.Status409Conflict, "Plans conflict."));
        PlanSettingsModel model = new(service);

        JsonResult result = Assert.IsType<JsonResult>(await model.OnGetPlansAsync(CancellationToken.None));
        using JsonDocument json = SerializeValue(result);

        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Plans conflict.", json.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task PlanSettings_Cancellation_Propagates()
    {
        IPlanSettingsService service = Substitute.For<IPlanSettingsService>();
        using CancellationTokenSource source = new();
        source.Cancel();
        service.GetPlans(source.Token)
            .Returns(Task.FromCanceled<ServiceResult<List<PlanDto>>>(source.Token));
        PlanSettingsModel model = new(service);

        await Assert.ThrowsAsync<TaskCanceledException>(() => model.OnGetPlansAsync(source.Token));
    }

    [Fact]
    public async Task Settings_MissingUpload_ReturnsValidationEnvelope()
    {
        SettingsModel model = new(Substitute.For<IReportBackgroundService>());

        JsonResult result = Assert.IsType<JsonResult>(
            await model.OnPostUploadReportBackgroundAsync(null!, CancellationToken.None));
        using JsonDocument json = SerializeValue(result);

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Выберите файл для загрузки.", json.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task EquipmentDetails_InvalidId_ReturnsValidationWithoutApplicationCall()
    {
        CRMService.Web.Pages.Equipments.DetailsModel model = new(
            null!,
            Options.Create(new CRMService.Application.Models.ConfigClass.ApiEndpointOptions()));

        JsonResult result = Assert.IsType<JsonResult>(
            await model.OnPostUpdateFromCloudApiAsync(0, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Theory]
    [InlineData(42, 3, "/issues?page=3")]
    [InlineData(42, 0, "/issues?page=1")]
    public void IssueDetails_RouteInput_PreservesBackPage(int id, int page, string expectedBackUrl)
    {
        CRMService.Web.Pages.Issues.DetailsModel model = new(null!);

        model.OnGet(id, page);

        Assert.Equal(id, model.IssueId);
        Assert.Equal(expectedBackUrl, model.BackUrl);
    }

    [Fact]
    public void EquipmentDetails_RouteInput_BuildsStableUrls()
    {
        CRMService.Web.Pages.Equipments.DetailsModel model = new(
            null!,
            Options.Create(new CRMService.Application.Models.ConfigClass.ApiEndpointOptions
            {
                OkdeskDomainUrl = "https://okdesk.invalid/"
            }));

        model.OnGet(17, 4);

        Assert.Equal(17, model.EquipmentId);
        Assert.Equal("/equipments?page=4", model.BackUrl);
        Assert.Equal("https://okdesk.invalid/equipments/17", model.OkdeskEquipmentUrl);
    }

    private static ReportModel CreateReportModel(IEmployeePerformanceReportService reportService) =>
        new(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            reportService,
            null!,
            null!,
            null!);

    private static JsonDocument SerializeValue(JsonResult result) =>
        JsonDocument.Parse(JsonSerializer.Serialize(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
}
