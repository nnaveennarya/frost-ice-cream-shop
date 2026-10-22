using System.Text.Json.Serialization;

namespace IceCreamShop.Desktop.Models;

public sealed record PageMetadata(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    int Total);
