using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SysMax2._1.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Converts a boolean to Visibility (true -> Visible, false -> Collapsed)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        // Converts back from Visibility to boolean.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                return v == Visibility.Visible;
            }
            return false;
        }
    }
}
