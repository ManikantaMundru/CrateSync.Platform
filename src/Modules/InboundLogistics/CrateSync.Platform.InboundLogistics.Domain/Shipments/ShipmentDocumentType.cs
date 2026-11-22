namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum ShipmentDocumentType
{
    BillOfLading = 1,
    CommercialInvoice = 2,
    PackingList = 3,
    BillOfEntry = 4,
    CertificateOfOrigin = 5,
    PhytosanitaryCertificate = 6,
    ImportPermit = 7,
    FoodSafetyNoc = 8,
    PlantQuarantineRelease = 9,
    CustomsOutOfCharge = 10,
    TreatmentCertificate = 11,
    Other = 12
}
