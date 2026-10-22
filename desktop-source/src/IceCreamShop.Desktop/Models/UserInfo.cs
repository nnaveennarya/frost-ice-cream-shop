namespace IceCreamShop.Desktop.Models;

public sealed record UserInfo(long Id, string Name, string Role, Dictionary<string, bool> Capabilities)
{
    public bool Can(string action) => Capabilities.TryGetValue(action, out var allowed) && allowed;
}
