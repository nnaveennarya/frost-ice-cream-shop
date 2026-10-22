using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IceCreamShop.Desktop.Services.Storage;

namespace IceCreamShop.Desktop.Services.Api;

public sealed class ApiTransport(HttpClient http, ITokenStore tokens)
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);

    public async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null,
        int? version = null, CancellationToken cancellation = default)
    {
        using var response = await SendCoreAsync(method, path, body, version, cancellation);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellation)
            ?? throw new InvalidDataException("Server returned an empty response.");
    }

    public async Task SendEmptyAsync(HttpMethod method, string path, int? version = null,
        CancellationToken cancellation = default)
    {
        using var response = await SendCoreAsync(method, path, null, version, cancellation);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string path,
        object? body, int? version, CancellationToken cancellation)
    {
        if (tokens.AccessToken is null || tokens.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new ApiFailure(HttpStatusCode.Unauthorized, "Session expired. Sign in again.");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        if (version is not null) request.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{version}\""));
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        else if (method == HttpMethod.Post) request.Content = JsonContent.Create(new Dictionary<string, object>());
        var response = await http.SendAsync(request, cancellation);
        if (response.IsSuccessStatusCode) return response;
        var failure = await ReadFailureAsync(response, cancellation);
        response.Dispose();
        throw failure;
    }

    private static async Task<ApiFailure> ReadFailureAsync(HttpResponseMessage response, CancellationToken cancellation)
    {
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Session expired. Sign in again.",
            HttpStatusCode.Forbidden => "Your role does not allow this action.",
            HttpStatusCode.Conflict => "This record changed. Reload it before saving or deleting.",
            HttpStatusCode.TooManyRequests => "Too many requests. Wait briefly and try again.",
            _ => "Request failed. Check the record and try again."
        };
        var fields = new Dictionary<string, string[]>();
        try
        {
            using var data = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellation), cancellationToken: cancellation);
            if (data.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("fields", out var values) && values.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in values.EnumerateObject())
                    fields[field.Name] = field.Value.ValueKind == JsonValueKind.Array
                        ? field.Value.EnumerateArray().Select(x => x.GetString() ?? "Invalid value.").ToArray()
                        : ["Invalid value."];
            }
        }
        catch (JsonException) { /* Never surface an HTML error page or server trace. */ }
        return new ApiFailure(response.StatusCode, message, fields);
    }
}
