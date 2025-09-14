using System;
using System.Globalization;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 可空 TimeSpan 到秒数字符串的双向转换器。
    /// 用于在 TextBox 和 TimeSpan? 之间进行转换。
    /// </summary>
    public class NullableTimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
                {
                    return TimeSpan.FromSeconds(seconds);
                }
            }
            return null; // Return null for invalid or empty input
        }
    }
}