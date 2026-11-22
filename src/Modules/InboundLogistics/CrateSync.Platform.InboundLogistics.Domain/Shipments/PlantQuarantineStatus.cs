namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum PlantQuarantineStatus
{
    NotRequired = 1,
    Pending = 2,
    DocumentsSubmitted = 3,
    InspectionScheduled = 4,
    UnderInspection = 5,
    Sampling = 6,
    TreatmentRequired = 7,
    Cleared = 8,
    Rejected = 9
}
