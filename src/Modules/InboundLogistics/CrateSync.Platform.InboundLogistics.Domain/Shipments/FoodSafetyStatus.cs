namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum FoodSafetyStatus
{
    NotRequired = 1,
    Pending = 2,
    Referred = 3,
    DocumentScrutiny = 4,
    InspectionScheduled = 5,
    VisualInspection = 6,
    Sampling = 7,
    LabTesting = 8,
    NocIssued = 9,
    NonConformance = 10,
    Rejected = 11
}
