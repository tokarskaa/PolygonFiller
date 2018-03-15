using System;
using System.Windows.Data;

namespace PolygonFiller
{
    class ComparisonConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return ((Enum)value).HasFlag((Enum)parameter);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;
            }
    }
}