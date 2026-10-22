using System.IO;
using System.Text.Json;

namespace IceCreamShop.Desktop.Models;

public sealed record AppSettings(string ApiBaseUrl, string ClientId, string RedirectUri)
{
    public static AppSettings Load(string directory)
    {
        var local = Path.Combine(directory, "appsettings.local.json");
        var path = File.Exists(local) ? local : Path.Combine(directory, "appsettings.json");
        var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Application settings are missing.");
        settings.Validate();
        return settings;
    }

    public void Validate()
    {
        if (!Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var origin)
            || origin.UserInfo.Length > 0 || origin.Query.Length > 0 || origin.Fragment.Length > 0
            || origin.AbsolutePath != "/"
            || (origin.Scheme != "https" && !(origin.Scheme == "http" && origin.IsLoopback)))
            throw new InvalidDataException("API origin must use HTTPS, or HTTP on a local development host.");
        if (!Uri.TryCreate(RedirectUri, UriKind.Absolute, out var callback)
            || callback.Scheme != "http" || callback.Host != "127.0.0.1"
            || callback.AbsolutePath != "/callback/" || callback.Query.Length > 0
            || callback.Fragment.Length > 0 || callback.UserInfo.Length > 0 || callback.Port < 1024)
            throw new InvalidDataException("OAuth callback must be a registered IPv4 loopback /callback/ address.");
    }
}
