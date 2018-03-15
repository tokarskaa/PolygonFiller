using System;
using System.Windows.Data;
using System.Windows.Media;

namespace PolygonFiller
{
    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value;
            return Colors.White;
        }
    }
}