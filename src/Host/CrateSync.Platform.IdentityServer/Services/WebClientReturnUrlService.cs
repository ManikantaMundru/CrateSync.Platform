namespace CrateSync.Platform.IdentityServer.Services;

public sealed class WebClientReturnUrlService(
    IConfiguration configuration)
{
    private readonly Uri _webBaseUri =
        new(
            configuration[
                "Clients:CrateSyncWeb:BaseUrl"]
            ?? "http://localhost:5173");

    public bool IsAllowed(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !Uri.TryCreate(
                returnUrl,
                UriKind.Absolute,
                out var returnUri))
        {
            return false;
        }

        return string.Equals(
                   returnUri.Scheme,
                   _webBaseUri.Scheme,
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   returnUri.Host,
                   _webBaseUri.Host,
                   StringComparison.OrdinalIgnoreCase) &&
               returnUri.Port == _webBaseUri.Port;
    }
}
