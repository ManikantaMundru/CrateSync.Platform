using System.Text.Json.Serialization;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClearanceAuthority
{
    Customs = 1,
    PlantQuarantine = 2,
    FoodSafety = 3,
    Port = 4,
    Other = 5
}
