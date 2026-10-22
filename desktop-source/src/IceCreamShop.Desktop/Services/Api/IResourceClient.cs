using IceCreamShop.Desktop.Models;

namespace IceCreamShop.Desktop.Services.Api;

public interface IResourceClient<T> where T : IVersionedResource
{
    Task<PageResult<T>> ListAsync(int page, string search, CancellationToken cancellation = default);
    Task<T> SaveAsync(long? id, object body, CancellationToken cancellation = default);
    Task DeleteAsync(long id, int version, CancellationToken cancellation = default);
}
