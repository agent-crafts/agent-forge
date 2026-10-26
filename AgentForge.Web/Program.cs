using AgentForge.Application;
using AgentForge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorOptions(options =>
{
    options.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");
});
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
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapGet("/", () => Results.Redirect("/chat"));
app.MapControllers();
app.Run();
