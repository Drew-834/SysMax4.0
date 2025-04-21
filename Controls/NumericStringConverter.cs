using System;
using System.Globalization;
using System.Windows.Data;

namespace SysMax2._1.Controls
{
    public class NumericStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts numeric values to formatted strings based on the converter parameter.
        /// Supported parameters:
        ///   "OneDecimal" - Rounds to one decimal place.
        ///   "ZeroDecimal" - Rounds to an integer.
        ///   "Temperature" - Rounds to an integer and appends "°C".
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            double number;
            try
            {
                number = System.Convert.ToDouble(value);
            }
            catch
            {
                return value.ToString();
            }

            string param = parameter as string;
            if (string.IsNullOrEmpty(param))
            {
                return number.ToString(culture);
            }
            else if (param.Equals("OneDecimal", StringComparison.OrdinalIgnoreCase))
            {
                // Round to one decimal place.
                return Math.Round(number, 1).ToString("0.0", culture);
            }
            else if (param.Equals("ZeroDecimal", StringComparison.OrdinalIgnoreCase))
            {
                // Round to no decimal places.
                return Math.Round(number, 0).ToString("0", culture);
            }
            else if (param.Equals("Temperature", StringComparison.OrdinalIgnoreCase))
            {
                // Round to integer and append the degree symbol and "C".
                return Math.Round(number, 0).ToString("0", culture) + "°C";
            }
            else
            {
                return number.ToString(culture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Remove known units or formatting based on the original parameter
                stringValue = stringValue.Replace("°C", "").Trim();
                // Add more replacements here if other formats are added (e.g., "%", " GB")
                
                if (double.TryParse(stringValue, NumberStyles.Any, culture, out double result))
                {
                    // Try to convert to the target type (e.g., int, double)
                    try 
                    {
                        return System.Convert.ChangeType(result, targetType);
                    }
                    catch
                    {
                        return Binding.DoNothing; // Conversion to target type failed
                    }
                }
            }
            // If parsing fails or input is not a string, indicate that the value cannot be converted
            return Binding.DoNothing;
        }
    }
}
