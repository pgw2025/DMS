using System.Globalization;
using System.Windows.Data;

namespace DMS.WPF.ValueConverts
{
    public class NullableBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string paramString)
            {
                if (bool.TryParse(paramString, out bool paramBool))
                {
                    return b == paramBool;
                }
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter is string paramString)
            {
                if (bool.TryParse(paramString, out bool paramBool))
                {
                    return paramBool;
                }
            }
            return Binding.DoNothing;
        }
    }
}