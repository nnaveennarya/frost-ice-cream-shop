using System.Net.Http;
using IceCreamShop.Desktop.Models;

namespace IceCreamShop.Desktop.Services.Api;

public sealed class ResourceClient<T>(ApiTransport api, string resource, bool supportsSearch = false)
    : IResourceClient<T> where T : IVersionedResource
{
    public Task<PageResult<T>> ListAsync(int page, string search, CancellationToken cancellation = default)
    {
        var query = $"api/{resource}?page={page}&per_page=20&sort=name";
        if (supportsSearch && !string.IsNullOrWhiteSpace(search)) query += "&search=" + Uri.EscapeDataString(search.Trim());
        return api.SendAsync<PageResult<T>>(HttpMethod.Get, query, cancellation: cancellation);
    }

    public async Task<T> SaveAsync(long? id, object body, CancellationToken cancellation = default)
    {
        var result = await api.SendAsync<ApiEnvelope<T>>(id is null ? HttpMethod.Post : HttpMethod.Patch,
            $"api/{resource}" + (id is null ? "" : $"/{id}"), body, cancellation: cancellation);
        return result.Data;
    }

    public Task DeleteAsync(long id, int version, CancellationToken cancellation = default) =>
        api.SendEmptyAsync(HttpMethod.Delete, $"api/{resource}/{id}", version, cancellation);
}
