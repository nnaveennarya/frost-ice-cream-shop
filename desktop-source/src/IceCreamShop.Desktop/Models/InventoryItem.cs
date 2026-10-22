using System.Text.Json.Serialization;

namespace IceCreamShop.Desktop.Models;

public sealed record InventoryItem : IVersionedResource
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    [JsonPropertyName("category_id")] public long CategoryId { get; init; }
    public Category? Category { get; init; }
    public string Price { get; init; } = "0.00";
    public uint Quantity { get; init; }
    public int Version { get; init; }
}
