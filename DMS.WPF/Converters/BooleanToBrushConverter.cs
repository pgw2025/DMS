using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DMS.WPF.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        // Predefined "True" color
        private static readonly Color DefaultTrueColor = Color.FromArgb(0xFF, 0xA3, 0xE4, 0xD7); // Mint Green

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is provided, try to use it as the "True" color
                string param = parameter as string;
                if (!string.IsNullOrEmpty(param))
                {
                    // Split the parameter by '|' to see if it contains two colors
                    string[] colors = param.Split('|');
                    
                    try
                    {
                        if (colors.Length == 2)
                        {
                            // Two colors: TrueColor|FalseColor
                            Color trueColor = (Color)ColorConverter.ConvertFromString(colors[0]);
                            if (colors[1].Equals("Default", StringComparison.OrdinalIgnoreCase))
                            {
                                // For false, return UnsetValue to let the control use its default background
                                return boolValue ? new SolidColorBrush(trueColor) : 
                                       System.Windows.DependencyProperty.UnsetValue;
                            }
                            else
                            {
                                Color falseColor = (Color)ColorConverter.ConvertFromString(colors[1]);
                                return boolValue ? new SolidColorBrush(trueColor) : 
                                       new SolidColorBrush(falseColor);
                            }
                        }
                        else if (colors.Length == 1)
                        {
                            // One color: TrueColor
                            Color trueColor = (Color)ColorConverter.ConvertFromString(colors[0]);
                            if (boolValue)
                            {
                                return new SolidColorBrush(trueColor);
                            }
                            else
                            {
                                // For false, return UnsetValue to let the control use its default background
                                return System.Windows.DependencyProperty.UnsetValue;
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        // If color format is invalid, fall back to default colors
                    }
                }

                // Default behavior
                if (boolValue)
                {
                    return new SolidColorBrush(DefaultTrueColor);
                }
                else
                {
                    // For false, return UnsetValue to let the control use its default background
                    return System.Windows.DependencyProperty.UnsetValue;
                }
            }
            
            // If value is not a boolean, return UnsetValue
            return System.Windows.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}