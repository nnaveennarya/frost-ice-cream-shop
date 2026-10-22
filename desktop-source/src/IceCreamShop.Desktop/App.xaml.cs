using System.Net.Http;
using System.Windows;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services;
using IceCreamShop.Desktop.Services.Api;
using IceCreamShop.Desktop.Services.Auth;
using IceCreamShop.Desktop.Services.Storage;
using IceCreamShop.Desktop.ViewModels;

namespace IceCreamShop.Desktop;

public partial class App : Application
{
    private HttpClient? _apiHttp;
    private HttpClient? _authHttp;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var settings = AppSettings.Load(AppContext.BaseDirectory);
            _apiHttp = CreateHttp(settings);
            _authHttp = CreateHttp(settings);
            var tokens = new InMemoryTokenStore();
            var api = new ApiTransport(_apiHttp, tokens);
            var oauth = new OAuthService(settings, _authHttp, new SystemBrowser(), tokens);
            var model = new MainViewModel(oauth, api, tokens, new WindowInteractionService());
            var window = new MainWindow { DataContext = model };
            model.SignInCompleted += window.BringToFront;
            MainWindow = window;
            MainWindow.Show();
        }
        catch (Exception error) when (error is System.IO.IOException or System.IO.InvalidDataException or System.Text.Json.JsonException or ArgumentException)
        {
            MessageBox.Show("Frost could not load valid settings. Check appsettings.json or appsettings.local.json.\n\n" + error.Message,
                "Configuration error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static HttpClient CreateHttp(AppSettings settings) => new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        BaseAddress = new Uri(settings.ApiBaseUrl),
        Timeout = TimeSpan.FromSeconds(20)
    };

    protected override void OnExit(ExitEventArgs e)
    {
        _apiHttp?.Dispose(); _authHttp?.Dispose(); base.OnExit(e);
    }
}

