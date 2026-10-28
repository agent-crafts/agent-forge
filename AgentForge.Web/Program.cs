using AgentForge.Application;
using AgentForge.Infrastructure;
using AgentForge.Core.Services;
using AgentForge.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMediator(options =>
{
    options.Assemblies = [typeof(AgentForge.Application.DependencyInjection)];
    options.ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped;
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/chat/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serves user-uploaded agent icons saved after deployment.
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapGet("/", () => Results.Redirect("/chat"));
app.MapControllers();
app.Run();
