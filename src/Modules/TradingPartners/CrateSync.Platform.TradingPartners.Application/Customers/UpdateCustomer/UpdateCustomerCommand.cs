using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string? CommercialIdentifier,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country,
    string? ContactName,
    string? Email,
    string? Phone)
    : ICommand<Result>;
