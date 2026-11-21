using System.Text.Json.Serialization;

namespace CrateSync.Platform.Identity.Domain.Authorization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TenantRole
{
    TenantAdmin = 1,
    Viewer = 5
}
