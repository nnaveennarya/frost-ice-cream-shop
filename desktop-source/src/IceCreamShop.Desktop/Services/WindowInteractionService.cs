using System.Windows;

namespace IceCreamShop.Desktop.Services;

public sealed class WindowInteractionService : IInteractionService
{
    public bool Confirm(string message, string title) => MessageBox.Show(message, title,
        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
}
