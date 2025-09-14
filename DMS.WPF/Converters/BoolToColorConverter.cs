using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 布尔值到颜色转换器。根据布尔值返回不同的颜色。
    /// 参数格式: "TrueColor;FalseColor"
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                string param = parameter as string;
                if (!string.IsNullOrEmpty(param))
                {
                    string[] colors = param.Split(';');
                    if (colors.Length == 2)
                    {
                        try
                        {
                            string colorString = boolValue ? colors[0] : colors[1];
                            Color color = (Color)ColorConverter.ConvertFromString(colorString);
                            return new SolidColorBrush(color);
                        }
                        catch (FormatException)
                        {
                            // 如果颜色格式无效，返回默认颜色
                        }
                    }
                }
                
                // 默认颜色
                Color defaultColor = boolValue ? Colors.Green : Colors.Red;
                return new SolidColorBrush(defaultColor);
            }

            // 如果值不是布尔值，返回红色
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}