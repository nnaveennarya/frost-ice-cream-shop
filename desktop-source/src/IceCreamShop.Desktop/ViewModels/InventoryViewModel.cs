using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using IceCreamShop.Desktop.Models;
using IceCreamShop.Desktop.Services;
using IceCreamShop.Desktop.Services.Api;

namespace IceCreamShop.Desktop.ViewModels;

public sealed class InventoryViewModel(IResourceClient<InventoryItem> client, ApiTransport api,
    IInteractionService interaction, Action sessionExpired)
    : ResourceViewModel<InventoryItem>(client, interaction, sessionExpired)
{
    private string _name = "", _description = "", _price = "", _quantity = "";
    private Category? _category;
    public ObservableCollection<Category> Categories { get; } = [];
    public string Name { get => _name; set => SetEditorProperty(ref _name, value, nameof(Name)); }
    public string Description { get => _description; set => SetEditorProperty(ref _description, value, nameof(Description)); }
    public string Price { get => _price; set => SetEditorProperty(ref _price, value, nameof(Price)); }
    public string Quantity { get => _quantity; set => SetEditorProperty(ref _quantity, value, nameof(Quantity)); }
    public Category? Category { get => _category; set => SetEditorProperty(ref _category, value, nameof(Category)); }
    public string NameError => Error("name");
    public string PriceError => Error("price");
    public string QuantityError => Error("quantity");
    public string CategoryError => Error("category_id");
    public string DescriptionError => Error("description");

    public async Task LoadCategoriesAsync(CancellationToken cancellation = default)
    {
        var result = await api.SendAsync<ApiEnvelope<List<Category>>>(HttpMethod.Get, "api/categories", cancellation: cancellation);
        Categories.Clear();
        foreach (var category in result.Data) Categories.Add(category);
    }

    protected override void LoadEditor(InventoryItem? item)
    {
        Name = item?.Name ?? "";
        Description = item?.Description ?? "";
        Price = item?.Price ?? "";
        Quantity = item?.Quantity.ToString(CultureInfo.InvariantCulture) ?? "";
        Category = Categories.FirstOrDefault(x => x.Id == item?.CategoryId);
    }

    protected override object? BuildBody()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 120) AddError("name", "Enter a name up to 120 characters.");
        if (Description.Length > 1000) AddError("description", "Use up to 1,000 characters.");
        if (!decimal.TryParse(Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)
            || price < 0 || price > 99999999.99m || decimal.Round(price, 2) != price)
            AddError("price", "Enter a nonnegative amount with up to two decimals, using a decimal point.");
        if (!uint.TryParse(Quantity, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity))
            AddError("quantity", "Enter a whole number from 0 to 4,294,967,295.");
        if (Category is null) AddError("category_id", "Choose a flavor.");
        if (Errors.Count > 0) return null;
        var body = new Dictionary<string, object?>
        {
            ["name"] = Name.Trim(),
            ["description"] = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            ["price"] = price.ToString("0.00", CultureInfo.InvariantCulture),
            ["quantity"] = quantity,
            ["category_id"] = Category!.Id
        };
        if (SelectedItem is not null) body["version"] = SelectedItem.Version;
        return body;
    }

    protected override void NotifyErrors()
    {
        foreach (var property in new[] { nameof(NameError), nameof(PriceError), nameof(QuantityError), nameof(CategoryError), nameof(DescriptionError) })
            OnPropertyChanged(property);
    }
}
