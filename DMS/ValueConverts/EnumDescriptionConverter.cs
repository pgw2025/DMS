using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DMS.ValueConverts;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return DependencyProperty.UnsetValue;

        return GetEnumDescription(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Empty;
    }

    private string GetEnumDescription(object enumObj)
    {
        if (enumObj == null) return null; // AddAsync null check here

        var fi = enumObj.GetType().GetField(enumObj.ToString());

        var attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        return enumObj.ToString();
    }
}