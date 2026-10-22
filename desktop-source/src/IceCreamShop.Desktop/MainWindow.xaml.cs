using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using IceCreamShop.Desktop.ViewModels;
namespace IceCreamShop.Desktop;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _expiry = new() { Interval = TimeSpan.FromSeconds(5) };
    public MainWindow()
    {
        InitializeComponent();
        _expiry.Tick += (_, _) => (DataContext as MainViewModel)?.CheckExpiry();
        Loaded += (_, _) => _expiry.Start();
        Closed += (_, _) => _expiry.Stop();
        Closing += ConfirmClose;
    }

    public void BringToFront()
    {
        if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        Show();
        Topmost = true;
        Topmost = false;
        Activate();
        Focus();
    }

    private void ConfirmClose(object? sender, CancelEventArgs e)
    {
        if (DataContext is MainViewModel model && !model.ConfirmDiscard()) e.Cancel = true;
    }
}
