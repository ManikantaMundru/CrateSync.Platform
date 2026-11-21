using System.Text.Json.Serialization;

namespace CrateSync.Platform.Identity.Domain.Authorization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WarehouseAccessRole
{
    WarehouseUser = 1,
    WarehouseAdmin = 2
}
