namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum CustomsClearanceStatus
{
    NotStarted = 1,
    BillOfEntryFiled = 2,
    Assessment = 3,
    Examination = 4,
    QueryRaised = 5,
    Held = 6,
    Cleared = 7,
    OutOfCharge = 8,
    Rejected = 9
}
