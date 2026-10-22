using System.Net;

namespace IceCreamShop.Desktop.Services.Api;

public sealed class ApiFailure(HttpStatusCode status, string message,
    Dictionary<string, string[]>? fields = null) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    public Dictionary<string, string[]> Fields { get; } = fields ?? [];
}
