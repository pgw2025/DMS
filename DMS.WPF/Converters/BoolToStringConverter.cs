using System;
using System.Globalization;
using System.Windows.Data;

namespace DMS.WPF.Converters
{
    /// <summary>
    /// 布尔值到字符串转换器。根据布尔值返回不同的字符串。
    /// 参数格式: "TrueString;FalseString"
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                string param = parameter as string;
                if (!string.IsNullOrEmpty(param))
                {
                    string[] strings = param.Split(';');
                    if (strings.Length == 2)
                    {
                        return boolValue ? strings[0] : strings[1];
                    }
                }
                
                // 默认返回
                return boolValue ? "是" : "否";
            }

            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}