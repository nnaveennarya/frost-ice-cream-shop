namespace IceCreamShop.Desktop.Models;

public interface IVersionedResource
{
    long Id { get; }
    int Version { get; }
}
