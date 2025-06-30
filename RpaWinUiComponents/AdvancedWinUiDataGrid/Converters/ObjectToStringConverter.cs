//Converters/ObjectToStringConverter.cs
using Microsoft.UI.Xaml.Data;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            if (value is DateTime dateTime)
                return dateTime.ToString("dd.MM.yyyy HH:mm");

            if (value is decimal || value is double || value is float)
                return string.Format("{0:N2}", value);

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var stringValue = value?.ToString();

            if (string.IsNullOrEmpty(stringValue))
                return null;

            // Try to convert back to the target type
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return int.TryParse(stringValue, out int intValue) ? intValue : null;
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                return double.TryParse(stringValue, out double doubleValue) ? doubleValue : null;
            }

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                return decimal.TryParse(stringValue, out decimal decimalValue) ? decimalValue : null;
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                return DateTime.TryParse(stringValue, out DateTime dateValue) ? dateValue : null;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                return bool.TryParse(stringValue, out bool boolValue) ? boolValue : null;
            }

            return stringValue;
        }
    }
}