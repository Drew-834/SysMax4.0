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
            throw new NotImplementedException();
        }
    }
}
