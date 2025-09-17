using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// Null值到可见性转换器。当绑定的值不为null时，返回Visible，否则返回Collapsed。
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果值不为null，则可见
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}