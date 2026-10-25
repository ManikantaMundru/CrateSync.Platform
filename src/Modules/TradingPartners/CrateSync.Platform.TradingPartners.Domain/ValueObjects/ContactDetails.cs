namespace CrateSync.Platform.TradingPartners.Domain.ValueObjects;

public sealed record ContactDetails
{
    private ContactDetails() { }

    public ContactDetails(string? contactName, string? email, string? phone)
    {
        ContactName = Normalize(contactName, 150);
        Email = Normalize(email, 254);
        Phone = Normalize(phone, 50);

        if (Email is null && Phone is null) throw new ArgumentException("At least one contact method must be provided.");
    }

    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength) throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }
}
