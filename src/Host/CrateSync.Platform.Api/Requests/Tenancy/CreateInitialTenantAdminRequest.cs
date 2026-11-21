namespace CrateSync.Platform.Api.Requests.Tenancy;

public sealed record CreateInitialTenantAdminRequest(
    string Email,
    string? UserName,
    string FirstName,
    string LastName);
