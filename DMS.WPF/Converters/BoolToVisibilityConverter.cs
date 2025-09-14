using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 布尔值到可见性转换器。当绑定的布尔值为true时，返回Visible，否则返回Collapsed。
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}