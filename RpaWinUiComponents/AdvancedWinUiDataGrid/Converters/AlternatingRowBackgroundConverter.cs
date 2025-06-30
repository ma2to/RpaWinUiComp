//Converters/AlternatingRowBackgroundConverter.cs
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class AlternatingRowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isEvenRow && isEvenRow)
            {
                return new SolidColorBrush(Colors.LightGray) { Opacity = 0.1 };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}