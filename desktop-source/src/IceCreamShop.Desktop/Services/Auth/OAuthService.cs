using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services.Storage;

namespace IceCreamShop.Desktop.Services.Auth;

public sealed class OAuthService(AppSettings settings, HttpClient http, ISystemBrowser browser, ITokenStore tokens)
{
    public async Task SignInAsync(CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(settings.ClientId) || settings.ClientId == "configure-public-client-id")
            throw new InvalidDataException("Set the public OAuth client ID in appsettings.local.json before signing in.");
        var verifier = Pkce.RandomValue();
        var state = Pkce.RandomValue();
        var callback = new Uri(settings.RedirectUri);
        using var listener = new LoopbackCallback(callback);
        listener.Start();
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = settings.ClientId,
            ["redirect_uri"] = settings.RedirectUri,
            ["scope"] = "shop:access",
            ["prompt"] = "login",
            ["state"] = state,
            ["code_challenge"] = Pkce.Challenge(verifier),
            ["code_challenge_method"] = "S256"
        };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));
        browser.Open(new Uri(new Uri(settings.ApiBaseUrl), "oauth/authorize?" + Encode(query)));
        var code = await listener.ReceiveCodeAsync(state, timeout.Token);
        using var response = await http.PostAsync("oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = settings.ClientId,
            ["redirect_uri"] = settings.RedirectUri,
            ["code_verifier"] = verifier,
            ["code"] = code
        }), timeout.Token);
        if (!response.IsSuccessStatusCode) throw new InvalidDataException("The authorization code could not be exchanged. Sign in again.");
        var token = await response.Content.ReadFromJsonAsync<OAuthToken>(cancellationToken: timeout.Token)
            ?? throw new InvalidDataException("The authorization server returned no token.");
        if (!string.Equals(token.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsupported token type.");
        tokens.Set(token.AccessToken, token.ExpiresIn);
    }

    public void OpenBrowserSignOut() => browser.Open(new Uri(new Uri(settings.ApiBaseUrl), "auth/sign-out"));

    private static string Encode(Dictionary<string, string> query) =>
        string.Join('&', query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
}
