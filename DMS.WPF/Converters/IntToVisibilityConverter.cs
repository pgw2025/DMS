using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 将整数值转换为Visibility的转换器。
    /// 当值等于参数时，返回Visibility.Collapsed；否则返回Visibility.Visible。
    /// </summary>
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
            {
                return intValue == paramValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // 默认情况下，如果值为0则隐藏，否则显示
            if (value is int intValueDefault)
            {
                return intValueDefault == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}