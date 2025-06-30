//Converters/ValidationErrorToBorderThicknessConverter.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class ValidationErrorToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool hasError && hasError)
            {
                return new Thickness(2);
            }
            return new Thickness(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}