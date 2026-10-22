namespace IceCreamShop.Desktop.Models;

public sealed record PageResult<T>(List<T> Data, PageMetadata Meta);
