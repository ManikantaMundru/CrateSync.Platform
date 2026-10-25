using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;

internal sealed class GetSupplierQueryHandler : IQueryHandler<GetSupplierQuery, Result<SupplierResponse>>
{
    private readonly ITradingPartnersReadService _readService;
    private readonly ITenantContext _tenantContext;

    public GetSupplierQueryHandler(ITradingPartnersReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<SupplierResponse>> Handle(GetSupplierQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _readService.GetSupplierByIdAsync(_tenantContext.TenantId, request.SupplierId, cancellationToken);

        if (supplier is null) return Result<SupplierResponse>.Failure(SupplierErrors.NotFound(SupplierId.From(request.SupplierId)));

        return Result<SupplierResponse>.Success(supplier);
    }
}
