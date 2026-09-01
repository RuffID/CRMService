using System.Net;
using CRMService.Application.Common.Exceptions;
using CRMService.Domain.Models.Constants;
using CRMService.Infrastructure.Service.Requests;
using EFCoreLibrary.Abstractions.Entity;
using HttpClientLibrary.Abstractions;
using HttpClientLibrary.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Service.Requests;

public class GetOkdeskEntityServiceTests
{
    [Fact]
    public async Task GetAllItemsAsync_IdPagination_AdvancesFromLastIdentifier()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        List<string> links = new();
        client.GetAsync<List<ApiItem>>(Arg.Do<string>(links.Add), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(
                [new ApiItem { Id = 10 }, new ApiItem { Id = 11 }],
                [new ApiItem { Id = 12 }]);
        GetOkdeskEntityService service = CreateService(client);
        List<List<ApiItem>> pages = new();

        await foreach (List<ApiItem> page in service.GetAllItemsAsync<ApiItem>("https://okdesk.invalid/items?x=1", 0, 2, ct: TestContext.Current.CancellationToken))
            pages.Add(page);

        Assert.Equal(2, pages.Count);
        Assert.Contains("page[from_id]=0", links[0]);
        Assert.Contains("page[from_id]=11", links[1]);
    }

    [Fact]
    public async Task GetAllItemsAsync_PageNumberPagination_DoesNotMixCursorParameters()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        List<string> links = new();
        client.GetAsync<List<ApiItem>>(Arg.Do<string>(links.Add), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(
                [new ApiItem { Id = 10 }, new ApiItem { Id = 11 }],
                [new ApiItem { Id = 12 }]);
        GetOkdeskEntityService service = CreateService(client);

        await foreach (List<ApiItem> _ in service.GetAllItemsAsync<ApiItem>("https://okdesk.invalid/items?x=1", 0, 2, pageNubmer: 1, ct: TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal(2, links.Count);
        Assert.Contains("page[number]=1", links[0]);
        Assert.Contains("page[number]=2", links[1]);
        Assert.DoesNotContain("page[from_id]", links[0]);
        Assert.DoesNotContain("page[direction]", links[0]);
    }

    [Fact]
    public async Task GetRangeOfItemsAsync_ExcessiveLimit_UsesConfiguredMaximum()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        string? capturedLink = null;
        client.GetAsync<List<ApiItem>>(Arg.Do<string>(value => capturedLink = value), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateService(client).GetRangeOfItemsAsync<ApiItem>("https://okdesk.invalid/items?x=1", limit: long.MaxValue, ct: TestContext.Current.CancellationToken);

        Assert.Contains($"page[size]={LimitConstants.LIMIT_FOR_RETRIEVING_ENTITIES_FROM_API}", capturedLink);
    }

    [Fact]
    public async Task GetItemAsync_NotFound_MapsRemoteResourceNotFoundException()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        client.GetAsync<ApiItem>(Arg.Any<string>(), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ApiItem?>(CreateHttpFailure(HttpStatusCode.NotFound))!);

        await Assert.ThrowsAsync<RemoteResourceNotFoundException>(() =>
            CreateService(client).GetItemAsync<ApiItem>("https://okdesk.invalid/items/404", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetItemAsync_OtherHttpError_PropagatesFailure()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        client.GetAsync<ApiItem>(Arg.Any<string>(), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ApiItem?>(CreateHttpFailure(HttpStatusCode.BadGateway))!);

        await Assert.ThrowsAsync<HttpRequestFailedException>(() =>
            CreateService(client).GetItemAsync<ApiItem>("https://okdesk.invalid/items/1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetRangeOfItemsAsync_HttpError_PropagatesFailure()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        client.GetAsync<List<ApiItem>>(Arg.Any<string>(), Arg.Any<IDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<ApiItem>?>(CreateHttpFailure(HttpStatusCode.BadGateway))!);

        await Assert.ThrowsAsync<HttpRequestFailedException>(() =>
            CreateService(client).GetRangeOfItemsAsync<ApiItem>("https://okdesk.invalid/items?x=1", ct: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetRangeOfItemsAsync_CancelledDelay_PropagatesCancellationWithoutHttpCall()
    {
        IHttpApiClient client = Substitute.For<IHttpApiClient>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService(client).GetRangeOfItemsAsync<ApiItem>("https://okdesk.invalid/items?x=1", ct: cancellation.Token));
        Assert.Empty(client.ReceivedCalls());
    }

    private static GetOkdeskEntityService CreateService(IHttpApiClient client) =>
        new(client, NullLogger<GetOkdeskEntityService>.Instance);

    private static HttpRequestFailedException CreateHttpFailure(HttpStatusCode statusCode) =>
        new(new Uri("https://okdesk.invalid/failure"), statusCode, statusCode.ToString(), "failure");

    private class ApiItem : IEntity<int>
    {
        public int Id { get; set; }
    }
}
