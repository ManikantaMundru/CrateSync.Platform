using Microsoft.AspNetCore.Authorization;

namespace CrateSync.Platform.Api.Authorization;

public sealed class TenantAdminRequirement
    : IAuthorizationRequirement;
