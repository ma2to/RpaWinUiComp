//Converters/ProgressToPercentageConverter.cs
using Microsoft.UI.Xaml.Data;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Converters
{
    public class ProgressToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double progress)
            {
                return $"{progress:F0}%";
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}