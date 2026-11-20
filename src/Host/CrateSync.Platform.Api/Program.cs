using CrateSync.Platform.Api.ExceptionHandling;
using CrateSync.Platform.Api.Tenancy;
using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Behaviors;
using CrateSync.Platform.BuildingBlocks.Infrastructure;
using CrateSync.Platform.Catalog.Application;
using CrateSync.Platform.Catalog.Infrastructure;
using CrateSync.Platform.Finance.Application;
using CrateSync.Platform.Finance.Infrastructure;
using CrateSync.Platform.InboundLogistics.Application;
using CrateSync.Platform.InboundLogistics.Infrastructure;
using CrateSync.Platform.Inventory.Application;
using CrateSync.Platform.Inventory.Infrastructure;
using CrateSync.Platform.Sales.Application;
using CrateSync.Platform.Sales.Infrastructure;
using CrateSync.Platform.Tenancy.Application;
using CrateSync.Platform.Tenancy.Infrastructure;
using CrateSync.Platform.TradingPartners.Application;
using CrateSync.Platform.TradingPartners.Infrastructure;
using CrateSync.Platform.Warehousing.Application;
using CrateSync.Platform.Warehousing.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

builder.Services.AddTradingPartnersApplication();
builder.Services.AddTradingPartnersInfrastructure(builder.Configuration);

builder.Services.AddWarehousingApplication();
builder.Services.AddWarehousingInfrastructure(builder.Configuration);

builder.Services.AddInboundLogisticsApplication();
builder.Services.AddInboundLogisticsInfrastructure(builder.Configuration);

builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

builder.Services.AddSalesApplication();
builder.Services.AddSalesInfrastructure(builder.Configuration);

builder.Services.AddFinanceApplication();
builder.Services.AddFinanceInfrastructure(builder.Configuration);

builder.Services.AddTenancyApplication();
builder.Services.AddTenancyInfrastructure(builder.Configuration);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

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
app.UseMiddleware<TenantValidationMiddleware>();
app.MapControllers();

app.Run();

public partial class Program;
