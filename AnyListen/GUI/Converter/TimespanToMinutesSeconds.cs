using System;
using System.Globalization;
using System.Windows.Data;

namespace AnyListen.GUI.Converter
{
    class TimespanToMinutesSeconds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "00:00";
            TimeSpan time = (TimeSpan)value;

            return $"{(int) time.TotalMinutes:00}:{time:ss}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
