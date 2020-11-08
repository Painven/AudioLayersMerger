using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AudioLayersMerger.Infrastructure.Converters
{
    public class TrueToStrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value == true)
            {
                return TextDecorations.Strikethrough;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
