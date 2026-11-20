namespace CrateSync.Platform.Sales.Domain.ValueObjects;

public sealed record DeliveryAddress
{
    private DeliveryAddress()
    {
        AddressLine1 = null!;
        City = null!;
        Country = null!;
    }

    public DeliveryAddress(
        string addressLine1,
        string? addressLine2,
        string city,
        string? province,
        string? postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(addressLine1)) throw new ArgumentException("Address line 1 is required.", nameof(addressLine1));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.", nameof(country));

        AddressLine1 = addressLine1.Trim();
        AddressLine2 = addressLine2?.Trim();
        City = city.Trim();
        Province = province?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country.Trim();
    }

    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string? Province { get; }
    public string? PostalCode { get; }
    public string Country { get; }
}
