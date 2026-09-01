var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Application = "CrateSync.Platform",
        Status = "Running"
    });
});

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy"
    });
});

app.Run();

public partial class Program;
