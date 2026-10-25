namespace CrateSync.Platform.TradingPartners.Domain.ValueObjects;

public sealed record Address
{
    private Address() { }

    public Address(string line1, string? line2, string city, string? stateOrProvince, string postalCode, string country)
    {
        Line1 = ValidateRequired(line1, nameof(line1), 200);
        Line2 = ValidateOptional(line2, 200);
        City = ValidateRequired(city, nameof(city), 100);
        StateOrProvince = ValidateOptional(stateOrProvince, 100);
        PostalCode = ValidateRequired(postalCode, nameof(postalCode), 30);
        Country = ValidateRequired(country, nameof(country), 100);
    }

    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? StateOrProvince { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private static string ValidateRequired(string value, string parameterName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength) throw new ArgumentException($"{parameterName} cannot exceed {maxLength} characters.", parameterName);

        return trimmed;
    }

    private static string? ValidateOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength) throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }
}
