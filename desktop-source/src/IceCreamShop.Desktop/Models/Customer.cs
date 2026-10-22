namespace IceCreamShop.Desktop.Models;

public sealed record Customer : IVersionedResource
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Version { get; init; }
}
