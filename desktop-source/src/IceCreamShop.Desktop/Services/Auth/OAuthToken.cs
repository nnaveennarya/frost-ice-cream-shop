using System.Text.Json.Serialization;

namespace IceCreamShop.Desktop.Services.Auth;

public sealed record OAuthToken(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType);
