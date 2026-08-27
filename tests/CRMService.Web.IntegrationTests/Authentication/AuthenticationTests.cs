using System.Net;
using CRMService.Application.Models.ConfigClass;
using CRMService.Web.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CRMService.Web.IntegrationTests.Authentication;

public class AuthenticationTests
{
    [Fact]
    public async Task ApiWithoutAuthentication_ReturnsUnauthorized()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/test-probe/authorized",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Theory]
    [InlineData("/index")]
    [InlineData("/users")]
    [InlineData("/settings")]
    [InlineData("/report")]
    [InlineData("/plansettings")]
    [InlineData("/issues")]
    [InlineData("/equipments")]
    public async Task RazorPageWithoutAuthentication_RedirectsToLogin(string path)
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Uri location = Assert.IsType<Uri>(response.Headers.Location);
        string loginPath = location.IsAbsoluteUri
            ? location.AbsolutePath
            : location.OriginalString.Split('?', 2)[0];
        Assert.Equal("/login", loginPath, ignoreCase: true);
    }

    [Fact]
    public async Task InsufficientRole_ReturnsForbidden()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        AddTestIdentity(client, role: null);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/test-probe/admin",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AllowedRole_GetsAccessToApiAndRazorPage()
    {
        await using CrmWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        AddTestIdentity(client, "admin");
        client.DefaultRequestHeaders.Add("Cookie", ".CRMService.Cookies=test-ticket");

        using HttpResponseMessage apiResponse = await client.GetAsync(
            "/api/test-probe/admin",
            TestContext.Current.CancellationToken);
        using HttpResponseMessage razorResponse = await client.GetAsync(
            "/users",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, razorResponse.StatusCode);
    }

    [Fact]
    public void CookieAndJwtOptions_UseExpectedProductionContractsWithTestSigningKey()
    {
        using CrmWebApplicationFactory factory = new();
        using IServiceScope scope = factory.Services.CreateScope();
        IOptionsMonitor<CookieAuthenticationOptions> cookieOptions =
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        IOptionsMonitor<JwtBearerOptions> jwtOptions =
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();

        CookieAuthenticationOptions cookie = cookieOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme);
        JwtBearerOptions jwt = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal(".CRMService.Cookies", cookie.Cookie.Name);
        Assert.Equal("/login", cookie.LoginPath.Value);
        Assert.Equal("/accessdenied", cookie.AccessDeniedPath.Value);
        Assert.True(jwt.TokenValidationParameters.ValidateIssuer);
        Assert.True(jwt.TokenValidationParameters.ValidateAudience);
        Assert.True(jwt.TokenValidationParameters.ValidateIssuerSigningKey);
        SymmetricSecurityKey signingKey = Assert.IsType<SymmetricSecurityKey>(
            jwt.TokenValidationParameters.IssuerSigningKey);
        Assert.Equal(
            "test-only-signing-key-with-at-least-thirty-two-characters-1234567890",
            System.Text.Encoding.UTF8.GetString(signingKey.Key));
    }

    private static HttpClient CreateClient(CrmWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static void AddTestIdentity(HttpClient client, string? role)
    {
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.USER_HEADER, "test-user");
        if (role is not null)
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.ROLE_HEADER, role);
    }
}
