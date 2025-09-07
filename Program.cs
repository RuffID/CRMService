using CRMService.Middleware;
using CRMService.Core;
using Serilog;
using CRMService.Service.DataBase;
using CRMService.DataBase;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"))
    .AddJsonFile("appsettings.json");

Log.Logger = new LoggerConfiguration()
    .Enrich.With(new SimpleClassNameEnricher())
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.ConfigureServices(builder.Configuration);

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (IServiceScope scope = app.Services.CreateScope())
{
    DataBaseCheckUpService<ApplicationContext> dbCheckUp = scope.ServiceProvider.GetRequiredService<DataBaseCheckUpService<ApplicationContext>>();
    dbCheckUp.CheckOrUpdateDB();
}

/*app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies =
        {
            IPAddress.Parse("127.0.0.1"), // запуск вне docker
            IPAddress.Parse("172.18.0.1") // если nginx с точки зрения контейнера
        },
});*/

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();