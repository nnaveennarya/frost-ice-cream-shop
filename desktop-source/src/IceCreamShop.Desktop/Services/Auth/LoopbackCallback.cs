using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IceCreamShop.Desktop.Services.Auth;

public sealed class LoopbackCallback(Uri address) : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, address.Port);

    public void Start() => _listener.Start(1);

    public async Task<string> ReceiveCodeAsync(string state, CancellationToken cancellation)
    {
        using var client = await _listener.AcceptTcpClientAsync(cancellation);
        await using var stream = client.GetStream();
        using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        requestTimeout.CancelAfter(TimeSpan.FromSeconds(10));
        var buffer = new byte[8192];
        var used = 0;
        string headers;
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(used), requestTimeout.Token);
            if (read == 0) throw new InvalidDataException("Sign-in callback connection closed.");
            used += read;
            headers = Encoding.ASCII.GetString(buffer, 0, used);
            if (headers.Contains("\r\n\r\n", StringComparison.Ordinal)) break;
            if (used == buffer.Length) throw new InvalidDataException("Sign-in callback request is too large.");
        }
        var lines = headers.Split("\r\n", StringSplitOptions.None);
        var request = lines[0].Split(' ');
        try
        {
            if (request.Length != 3 || request[0] != "GET" || !request[1].StartsWith('/'))
                throw new InvalidDataException("Invalid sign-in callback request.");
            var hosts = lines.Where(x => x.StartsWith("Host:", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (hosts.Length != 1 || hosts[0][5..].Trim() != address.Authority)
                throw new InvalidDataException("Unexpected callback host.");
            var callback = new Uri(address, request[1]);
            var code = Pkce.ValidateCallback(callback, address, state);
            await RespondAsync(stream, true, cancellation);
            return code;
        }
        catch (InvalidDataException)
        {
            await RespondAsync(stream, false, cancellation);
            throw;
        }
    }

    private static async Task RespondAsync(NetworkStream stream, bool success, CancellationToken cancellation)
    {
        var body = success ? "Sign-in complete. Return to Frost. You can close this tab."
            : "Sign-in could not be verified. Return to Frost and try again.";
        var payload = $"HTTP/1.1 {(success ? "200 OK" : "400 Bad Request")}\r\nContent-Type: text/plain; charset=utf-8\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\nConnection: close\r\nContent-Length: {Encoding.UTF8.GetByteCount(body)}\r\n\r\n{body}";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(payload), cancellation);
    }

    public void Dispose() => _listener.Stop();
}
