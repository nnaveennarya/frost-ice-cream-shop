using System.Net.Mail;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services;
using IceCreamShop.Desktop.Services.Api;

namespace IceCreamShop.Desktop.ViewModels;

public sealed class CustomersViewModel(IResourceClient<Customer> client, IInteractionService interaction, Action sessionExpired)
    : ResourceViewModel<Customer>(client, interaction, sessionExpired)
{
    private string _name = "", _email = "", _phone = "";
    public string Name { get => _name; set => SetEditorProperty(ref _name, value, nameof(Name)); }
    public string Email { get => _email; set => SetEditorProperty(ref _email, value, nameof(Email)); }
    public string Phone { get => _phone; set => SetEditorProperty(ref _phone, value, nameof(Phone)); }
    public string NameError => Error("name");
    public string EmailError => Error("email");
    public string PhoneError => Error("phone");

    protected override void LoadEditor(Customer? item)
    {
        Name = item?.Name ?? ""; Email = item?.Email ?? ""; Phone = item?.Phone ?? "";
    }

    protected override object? BuildBody()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 120) AddError("name", "Enter a name up to 120 characters.");
        if (!string.IsNullOrWhiteSpace(Email) && (Email.Trim().Length > 254 || !MailAddress.TryCreate(Email.Trim(), out var parsed)
            || parsed.Address != Email.Trim())) AddError("email", "Enter a valid email address, or leave it empty.");
        if (Phone.Trim().Length > 32) AddError("phone", "Use up to 32 characters.");
        if (Errors.Count > 0) return null;
        var body = new Dictionary<string, object?>
        {
            ["name"] = Name.Trim(),
            ["email"] = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            ["phone"] = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim()
        };
        if (SelectedItem is not null) body["version"] = SelectedItem.Version;
        return body;
    }

    protected override void NotifyErrors()
    {
        OnPropertyChanged(nameof(NameError)); OnPropertyChanged(nameof(EmailError)); OnPropertyChanged(nameof(PhoneError));
    }
}
