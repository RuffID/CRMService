using System.Net;
using CRMService.Application.Models.ConfigClass;
using CRMService.Web.Core.Filter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CRMService.Web.IntegrationTests.Filters;

public class IpOkdeskWebHookActionFilterTests
{
    [Theory]
    [InlineData("203.0.113.10")]
    [InlineData("198.51.100.42")]
    public void OnActionExecuting_WhitelistedAddress_AllowsRequest(string address)
    {
        IpOkdeskWebHookActionFilterAttribute filter = CreateFilter("203.0.113.10", "198.51.100.0/24");
        ActionExecutingContext context = CreateContext(address);

        filter.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_NonWhitelistedAddress_ReturnsForbidden()
    {
        IpOkdeskWebHookActionFilterAttribute filter = CreateFilter("203.0.113.10");
        ActionExecutingContext context = CreateContext("192.0.2.10");

        filter.OnActionExecuting(context);

        StatusCodeResult result = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_MissingOptions_DeniesByDefault()
    {
        IpOkdeskWebHookActionFilterAttribute filter = CreateFilter();
        ActionExecutingContext context = CreateContext("203.0.113.10");

        filter.OnActionExecuting(context);

        StatusCodeResult result = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public void Constructor_MalformedAddress_FailsFast()
    {
        Assert.Throws<FormatException>(() => CreateFilter("not-an-ip-address"));
    }

    private static IpOkdeskWebHookActionFilterAttribute CreateFilter(params string[] addresses) =>
        new(
            NullLogger<IpOkdeskWebHookActionFilterAttribute>.Instance,
            Options.Create(new WebHookOkdeskOptions { IpAddressList = addresses }));

    private static ActionExecutingContext CreateContext(string address)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(address);
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }
}
