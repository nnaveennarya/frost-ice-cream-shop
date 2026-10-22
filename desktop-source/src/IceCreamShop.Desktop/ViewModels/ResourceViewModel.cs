using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services;
using IceCreamShop.Desktop.Services.Api;

namespace IceCreamShop.Desktop.ViewModels;

public abstract partial class ResourceViewModel<T> : ObservableObject where T : class, IVersionedResource
{
    private readonly IResourceClient<T> _client;
    private readonly IInteractionService _interaction;
    private readonly Action _sessionExpired;
    private CancellationTokenSource _pending = new();
    private int _generation;
    private int _page = 1;
    private int _lastPage = 1;
    private T? _selectedItem;
    private bool _loadingEditor;
    protected Dictionary<string, string[]> Errors = [];

    [ObservableProperty] private string _search = "";
    [ObservableProperty] private string _status = "Select a record, or add a new one.";
    [ObservableProperty] private string _pageSummary = "No records loaded";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _hasEditor;
    [ObservableProperty] private bool _canCreate;
    [ObservableProperty] private bool _canUpdate;
    [ObservableProperty] private bool _canDelete;
    public ObservableCollection<T> Records { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand PreviousCommand { get; }
    public IAsyncRelayCommand NextCommand { get; }
    public IRelayCommand NewCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    protected ResourceViewModel(IResourceClient<T> client, IInteractionService interaction, Action sessionExpired)
    {
        _client = client;
        _interaction = interaction;
        _sessionExpired = sessionExpired;
        RefreshCommand = new AsyncRelayCommand(() => LoadAsync(), () => !IsBusy);
        SearchCommand = new AsyncRelayCommand(async () => { _page = 1; await LoadAsync(); }, () => !IsBusy);
        PreviousCommand = new AsyncRelayCommand(() => NavigateAsync(_page - 1), () => !IsBusy && _page > 1);
        NextCommand = new AsyncRelayCommand(() => NavigateAsync(_page + 1), () => !IsBusy && _page < _lastPage);
        NewCommand = new RelayCommand(New, () => !IsBusy && CanCreate);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy && HasEditor && (SelectedItem is null ? CanCreate : CanUpdate));
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsBusy && CanDelete && SelectedItem is not null);
    }

    public T? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Equals(_selectedItem, value)) return;
            if (IsBusy || !ConfirmDiscard()) { OnPropertyChanged(nameof(SelectedItem)); return; }
            SetProperty(ref _selectedItem, value);
            SetEditor(value);
        }
    }

    public bool CanEditFields => HasEditor && !IsBusy;

    public bool ConfirmDiscard()
    {
        if (!IsDirty) return true;
        if (!_interaction.Confirm("Discard unsaved changes?", "Unsaved changes")) return false;
        IsDirty = false;
        return true;
    }
    public void Configure(UserInfo user, string resource)
    {
        CanCreate = user.Can(resource + ".create");
        CanUpdate = user.Can(resource + ".update");
        CanDelete = user.Can(resource + ".delete");
        NotifyCommands();
    }

    public void Clear()
    {
        _generation++;
        _pending.Cancel();
        _pending.Dispose();
        _pending = new CancellationTokenSource();
        IsDirty = false;
        IsBusy = false;
        Records.Clear();
        _selectedItem = null;
        OnPropertyChanged(nameof(SelectedItem));
        SetEditor(null);
        HasEditor = false;
        Search = "";
        _page = _lastPage = 1;
        PageSummary = "No records loaded";
        CanCreate = CanUpdate = CanDelete = false;
        Status = "Sign in to load records.";
        NotifyCommands();
    }

    public async Task LoadAsync()
    {
        if (IsBusy || !ConfirmDiscard()) return;
        var generation = _generation;
        IsBusy = true;
        try
        {
            await ReloadPageAsync(generation);
        }
        catch (Exception error) when (IsExpected(error)) { if (generation == _generation) Handle(error); }
        finally { if (generation == _generation) IsBusy = false; }
    }

    private async Task ReloadPageAsync(int generation)
    {
        var result = await _client.ListAsync(_page, Search, _pending.Token);
        if (generation != _generation) return;
        CloseEditor();
        Records.Clear();
        foreach (var item in result.Data) Records.Add(item);
        _page = result.Meta.CurrentPage;
        _lastPage = Math.Max(1, result.Meta.LastPage);
        PageSummary = $"Page {_page} of {_lastPage} · {result.Meta.Total} records";
        Status = Records.Count == 0 ? "No records found. Try another search or add a record." : "Select a record to view and edit.";
    }

    private void CloseEditor()
    {
        IsDirty = false;
        _selectedItem = null;
        OnPropertyChanged(nameof(SelectedItem));
        SetEditor(null);
        HasEditor = false;
    }

    private async Task NavigateAsync(int page)
    {
        if (IsBusy || !ConfirmDiscard()) return;
        _page = page;
        await LoadAsync();
    }

    protected bool SetEditorProperty<TValue>(ref TValue field, TValue value, string property)
    {
        if (IsBusy && !_loadingEditor) return false;
        if (!SetProperty(ref field, value, property)) return false;
        if (!_loadingEditor) IsDirty = true;
        return true;
    }

    protected string Error(string field) => Errors.TryGetValue(field, out var errors) ? string.Join(" ", errors) : "";
    protected abstract void LoadEditor(T? item);
    protected abstract object? BuildBody();
    protected abstract void NotifyErrors();
    protected void AddError(string field, string message) => Errors[field] = [message];
    private void SetEditor(T? item)
    {
        _loadingEditor = true;
        LoadEditor(item);
        _loadingEditor = false;
        HasEditor = item is not null;
        IsDirty = false;
        Errors.Clear();
        NotifyErrors();
        NotifyCommands();
    }

    private void New()
    {
        if (IsBusy || !ConfirmDiscard()) return;
        _selectedItem = null;
        OnPropertyChanged(nameof(SelectedItem));
        SetEditor(null);
        HasEditor = true;
        Status = "New record. Complete the required fields, then save.";
    }

    private async Task SaveAsync()
    {
        if (IsBusy || !HasEditor || !(SelectedItem is null ? CanCreate : CanUpdate)) return;
        Errors.Clear();
        var body = BuildBody();
        NotifyErrors();
        if (body is null) { Status = "Correct the highlighted fields."; return; }
        await MutateAsync(async () => { await _client.SaveAsync(SelectedItem?.Id, body, _pending.Token); }, "Record saved.");
    }

    private async Task DeleteAsync()
    {
        var selected = SelectedItem;
        if (IsBusy || !CanDelete || selected is null || !_interaction.Confirm("Permanently delete this record? Unsaved changes will be lost.", "Delete record")) return;
        await MutateAsync(() => _client.DeleteAsync(selected.Id, selected.Version, _pending.Token), "Record deleted.");
    }

    private async Task MutateAsync(Func<Task> mutation, string success)
    {
        if (IsBusy) return;
        var generation = _generation;
        var committed = false;
        IsBusy = true;
        try
        {
            await mutation();
            if (generation != _generation) return;
            committed = true;
            CloseEditor();
            await ReloadPageAsync(generation);
            if (generation == _generation) Status = success;
        }
        catch (Exception error) when (IsExpected(error))
        {
            if (generation != _generation) return;
            Handle(error);
            if (committed && generation == _generation) Status = success + " List refresh failed. Reload to see the latest records.";
        }
        finally { if (generation == _generation) IsBusy = false; }
    }

    private void Handle(Exception error)
    {
        Status = error switch
        {
            ApiFailure api => api.Message,
            OperationCanceledException => "Request cancelled or timed out. Reload before retrying a write; it may have succeeded.",
            _ => "Connection failed. Check the backend, then reload before retrying a write."
        };
        if (error is not ApiFailure failure) return;
        Errors = failure.Fields;
        NotifyErrors();
        if (failure.Status == HttpStatusCode.Unauthorized) _sessionExpired();
    }

    private static bool IsExpected(Exception error) => error is ApiFailure or HttpRequestException
        or OperationCanceledException or System.Text.Json.JsonException or System.IO.InvalidDataException;
    partial void OnIsBusyChanged(bool value) { OnPropertyChanged(nameof(CanEditFields)); NotifyCommands(); }
    partial void OnHasEditorChanged(bool value) { OnPropertyChanged(nameof(CanEditFields)); NotifyCommands(); }
    private void NotifyCommands()
    {
        RefreshCommand.NotifyCanExecuteChanged(); SearchCommand.NotifyCanExecuteChanged();
        PreviousCommand.NotifyCanExecuteChanged(); NextCommand.NotifyCanExecuteChanged();
        NewCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged();
    }
}
