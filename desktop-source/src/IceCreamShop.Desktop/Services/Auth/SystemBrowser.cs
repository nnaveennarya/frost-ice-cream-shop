using System.Diagnostics;

namespace IceCreamShop.Desktop.Services.Auth;

public sealed class SystemBrowser : ISystemBrowser
{
    public void Open(Uri address)
    {
        if (address.Scheme is not ("http" or "https")) throw new ArgumentException("Unsupported browser address.");
        Process.Start(new ProcessStartInfo(address.AbsoluteUri) { UseShellExecute = true });
    }
}
