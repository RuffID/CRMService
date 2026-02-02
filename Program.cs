using CRMService.Core;
using Serilog;
using CRMService.Service.DataBase;
using CRMService.Core.Middleware;
using CRMService.DataBase;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"))
    .AddJsonFile("config.json");

Log.Logger = new LoggerConfiguration()
    .Enrich.With(new SimpleClassNameEnricher())
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.ConfigureServices(builder);

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (IServiceScope scope = app.Services.CreateScope())
{
    DataBaseCheckUpService<ApplicationContext> dbCheckUp = scope.ServiceProvider.GetRequiredService<DataBaseCheckUpService<ApplicationContext>>();
    dbCheckUp.CheckOrUpdateDB();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();