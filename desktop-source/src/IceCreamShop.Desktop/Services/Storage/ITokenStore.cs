namespace IceCreamShop.Desktop.Services.Storage;

public interface ITokenStore
{
    string? AccessToken { get; }
    DateTimeOffset ExpiresAt { get; }
    void Set(string token, int lifetimeSeconds);
    void Clear();
}
