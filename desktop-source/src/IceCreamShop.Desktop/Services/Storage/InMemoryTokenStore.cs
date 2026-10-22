namespace IceCreamShop.Desktop.Services.Storage;

public sealed class InMemoryTokenStore : ITokenStore
{
    public string? AccessToken { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public void Set(string token, int lifetimeSeconds)
    {
        if (string.IsNullOrWhiteSpace(token) || lifetimeSeconds <= 0)
            throw new ArgumentException("Token and lifetime must be valid.");
        AccessToken = token;
        ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds);
    }

    public void Clear()
    {
        AccessToken = null;
        ExpiresAt = DateTimeOffset.MinValue;
    }
}
