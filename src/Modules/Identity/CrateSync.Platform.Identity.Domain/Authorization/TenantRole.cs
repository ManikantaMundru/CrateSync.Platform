using System.Text.Json.Serialization;

namespace CrateSync.Platform.Identity.Domain.Authorization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TenantRole
{
    TenantAdmin = 1,
    Viewer = 5,
    TenantOwner = 6,
    Finance = 7
}
