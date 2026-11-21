using CrateSync.Platform.Identity.Infrastructure;
using CrateSync.Platform.IdentityServer;
using CrateSync.Platform.IdentityServer.Services;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services
    .AddCrateSyncIdentityInfrastructure(
        builder.Configuration);

builder.Services
    .AddCrateSyncIdentityServer(
        builder.Configuration,
        builder.Environment);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet(
    "/health",
    () => Results.Ok(
        new
        {
            Application =
                "CrateSync.Platform.IdentityServer",
            Status = "Healthy"
        }));

using (var scope = app.Services.CreateScope())
{
    var seeder =
        scope.ServiceProvider
            .GetRequiredService<
                BootstrapSuperAdminSeeder>();

    await seeder.SeedAsync();
}

app.Run();

public partial class Program;
