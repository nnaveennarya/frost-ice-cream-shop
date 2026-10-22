using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace IceCreamShop.Desktop.Services.Presentation;

public static class ValidationFocus
{
    public static readonly DependencyProperty StatusProperty = DependencyProperty.RegisterAttached(
        "Status", typeof(string), typeof(ValidationFocus), new PropertyMetadata("", Changed));
    public static void SetStatus(DependencyObject element, string value) => element.SetValue(StatusProperty, value);
    public static string GetStatus(DependencyObject element) => (string)element.GetValue(StatusProperty);

    private static void Changed(DependencyObject element, DependencyPropertyChangedEventArgs args)
    {
        if (element is not FrameworkElement root) return;
        root.Dispatcher.BeginInvoke(() =>
        {
            var first = Descendants(root).OfType<TextBlock>()
                .FirstOrDefault(x => Equals(x.Tag, "field-error") && !string.IsNullOrWhiteSpace(x.Text));
            if (first?.Parent is not Panel panel) return;
            var index = panel.Children.IndexOf(first);
            if (index > 0 && panel.Children[index - 1] is Control input)
            {
                input.Focus(); input.BringIntoView();
            }
        }, DispatcherPriority.Background);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (var item in Descendants(child)) yield return item;
        }
    }
}
