//Converters/BoolToVisibilityConverter.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = value is bool boolValue && boolValue;

            // Support for inverse parameter
            if (parameter?.ToString() == "Inverse")
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = value is Visibility visibility && visibility == Visibility.Visible;

            // Support for inverse parameter
            if (parameter?.ToString() == "Inverse")
            {
                isVisible = !isVisible;
            }

            return isVisible;
        }
    }
}