using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CodeCompletion.Controls;

internal class VisibleIf(Func<object?, bool> predicate) : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => predicate(value) ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    public static readonly VisibleIf NotNull = new(static x => x is not null);
}
