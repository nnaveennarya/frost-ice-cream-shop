using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services;
using IceCreamShop.Desktop.Services.Api;
using IceCreamShop.Desktop.Services.Auth;
using IceCreamShop.Desktop.Services.Storage;

namespace IceCreamShop.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly OAuthService _oauth;
    private readonly ApiTransport _api;
    private readonly ITokenStore _tokens;
    private CancellationTokenSource? _signIn;
    [ObservableProperty] private bool _isSignedIn;
    [ObservableProperty] private bool _isSigningIn;
    [ObservableProperty] private bool _isSigningOut;
    [ObservableProperty] private string _status = "Sign in securely to manage your shop.";
    [ObservableProperty] private string _userLabel = "";
    [ObservableProperty] private object? _currentPage;
    public InventoryViewModel Inventory { get; }
    public CustomersViewModel Customers { get; }
    public IAsyncRelayCommand SignInCommand { get; }
    public IRelayCommand CancelSignInCommand { get; }
    public IAsyncRelayCommand SignOutCommand { get; }
    public IAsyncRelayCommand ShowInventoryCommand { get; }
    public IAsyncRelayCommand ShowCustomersCommand { get; }
    public event Action? SignInCompleted;

    public MainViewModel(OAuthService oauth, ApiTransport api, ITokenStore tokens, IInteractionService interaction)
    {
        _oauth = oauth; _api = api; _tokens = tokens;
        Inventory = new(new ResourceClient<InventoryItem>(api, "items", true), api, interaction, ExpireSession);
        Customers = new(new ResourceClient<Customer>(api, "customers"), interaction, ExpireSession);
        SignInCommand = new AsyncRelayCommand(SignInAsync, () => !IsSigningIn && !IsSigningOut && !IsSignedIn);
        CancelSignInCommand = new RelayCommand(() => _signIn?.Cancel(), () => IsSigningIn);
        SignOutCommand = new AsyncRelayCommand(SignOutAsync, () => IsSignedIn && !IsSigningOut);
        ShowInventoryCommand = new AsyncRelayCommand(async () => { if (CanNavigate && ConfirmDiscard()) { CurrentPage = Inventory; await Inventory.LoadAsync(); } }, () => CanNavigate);
        ShowCustomersCommand = new AsyncRelayCommand(async () => { if (CanNavigate && ConfirmDiscard()) { CurrentPage = Customers; await Customers.LoadAsync(); } }, () => CanNavigate);
        Inventory.PropertyChanged += ResourceStateChanged;
        Customers.PropertyChanged += ResourceStateChanged;
    }

    private bool CanNavigate => IsSignedIn && !IsSigningOut && !Inventory.IsBusy && !Customers.IsBusy;
    private void ResourceStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(Inventory.IsBusy)) return;
        ShowInventoryCommand.NotifyCanExecuteChanged(); ShowCustomersCommand.NotifyCanExecuteChanged();
    }

    public bool ConfirmDiscard() => Inventory.ConfirmDiscard() && Customers.ConfirmDiscard();
    public void CheckExpiry()
    {
        if (IsSignedIn && _tokens.ExpiresAt <= DateTimeOffset.UtcNow) ExpireSession();
    }

    private async Task SignInAsync()
    {
        if (IsSigningIn || IsSigningOut || IsSignedIn) return;
        IsSigningIn = true;
        _signIn = new CancellationTokenSource();
        Status = "Complete sign-in in your browser. This window will update automatically.";
        try
        {
            await _oauth.SignInAsync(_signIn.Token);
            SignInCompleted?.Invoke();
            var result = await _api.SendAsync<ApiEnvelope<UserInfo>>(HttpMethod.Get, "api/auth/me", cancellation: _signIn.Token);
            await Inventory.LoadCategoriesAsync(_signIn.Token);
            Inventory.Configure(result.Data, "items"); Customers.Configure(result.Data, "customers");
            UserLabel = $"{result.Data.Name} · {result.Data.Role}";
            IsSignedIn = true;
            CurrentPage = Inventory;
            Status = "";
            await Inventory.LoadAsync();
        }
        catch (Exception error) when (error is HttpRequestException or ApiFailure or OperationCanceledException or InvalidDataException or SocketException or System.Text.Json.JsonException or System.ComponentModel.Win32Exception)
        {
            _tokens.Clear();
            Inventory.Clear(); Customers.Clear();
            Status = error switch
            {
                OperationCanceledException => "Sign-in cancelled or timed out. You can try again.",
                SocketException => "Another Frost window may be signing in. Finish its browser sign-in, then try again.",
                InvalidDataException => error.Message,
                _ => "Sign-in could not complete. Check the server and configuration, then try again."
            };
        }
        finally { _signIn.Dispose(); _signIn = null; IsSigningIn = false; }
    }

    private async Task SignOutAsync()
    {
        if (!IsSignedIn || IsSigningOut || !ConfirmDiscard()) return;
        IsSigningOut = true;
        var originatingToken = _tokens.AccessToken;
        try
        {
            var revoked = true;
            try { await _api.SendEmptyAsync(HttpMethod.Post, "api/auth/logout"); }
            catch (Exception error) when (error is HttpRequestException or ApiFailure or OperationCanceledException) { revoked = false; }
            // A stale completion must never clear a different account/session.
            if (_tokens.AccessToken is not null && _tokens.AccessToken != originatingToken) return;
            ExpireSession();
            Status = revoked ? "Signed out. Finish browser sign-out in the page that opens."
                : "Local session cleared. Server revocation could not be confirmed; token expires automatically. Finish browser sign-out when online.";
            try { _oauth.OpenBrowserSignOut(); }
            catch (System.ComponentModel.Win32Exception) { Status += " Open the server's /auth/sign-out page in your browser."; }
        }
        finally { IsSigningOut = false; }
    }

    private void ExpireSession()
    {
        _tokens.Clear(); Inventory.Clear(); Customers.Clear();
        IsSignedIn = false; CurrentPage = null; UserLabel = "";
        Status = "Your session ended. Sign in again to continue.";
    }

    partial void OnIsSigningInChanged(bool value)
    {
        SignInCommand.NotifyCanExecuteChanged(); CancelSignInCommand.NotifyCanExecuteChanged();
    }
    partial void OnIsSignedInChanged(bool value)
    {
        SignInCommand.NotifyCanExecuteChanged(); SignOutCommand.NotifyCanExecuteChanged();
        ShowInventoryCommand.NotifyCanExecuteChanged(); ShowCustomersCommand.NotifyCanExecuteChanged();
    }
    partial void OnIsSigningOutChanged(bool value)
    {
        SignInCommand.NotifyCanExecuteChanged(); SignOutCommand.NotifyCanExecuteChanged();
        ShowInventoryCommand.NotifyCanExecuteChanged(); ShowCustomersCommand.NotifyCanExecuteChanged();
    }
}
