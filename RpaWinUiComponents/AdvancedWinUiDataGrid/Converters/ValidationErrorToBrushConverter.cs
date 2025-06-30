
//Converters/ValidationErrorToBrushConverter.cs
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class ValidationErrorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool hasError && hasError)
            {
                return new SolidColorBrush(Colors.Red) { Opacity = 0.1 };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}