using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 计数到可见性转换器。当集合为空时，返回Visible，否则返回Collapsed。
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return collection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}