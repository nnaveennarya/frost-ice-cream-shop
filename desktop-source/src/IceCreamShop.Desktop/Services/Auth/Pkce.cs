using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IceCreamShop.Desktop.Services.Auth;

public static class Pkce
{
    public static string RandomValue() => Base64Url(RandomNumberGenerator.GetBytes(32));
    public static string Challenge(string verifier) => Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string ValidateCallback(Uri callback, Uri expected, string state)
    {
        if (callback.GetLeftPart(UriPartial.Path) != expected.GetLeftPart(UriPartial.Path))
            throw new InvalidDataException("Unexpected sign-in callback address.");
        var fields = ParseQuery(callback.Query);
        if (!fields.TryGetValue("state", out var received)
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(state), Encoding.UTF8.GetBytes(received)))
            throw new InvalidDataException("Sign-in state did not match. Start sign-in again.");
        if (fields.ContainsKey("error")) throw new InvalidDataException("Sign-in was declined or cancelled.");
        if (!fields.TryGetValue("code", out var code) || string.IsNullOrWhiteSpace(code))
            throw new InvalidDataException("Sign-in response did not include an authorization code.");
        return code;
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var values = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(values[0].Replace('+', ' '));
            var value = values.Length == 2 ? Uri.UnescapeDataString(values[1].Replace('+', ' ')) : "";
            if (!result.TryAdd(key, value)) throw new InvalidDataException("Ambiguous sign-in response.");
        }
        return result;
    }
}
