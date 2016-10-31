using System;
using System.Globalization;
using System.Windows.Data;
using AnyListen.Music.Track.WebApi;

namespace AnyListen.GUI.Converter
{
    class IsMusicApiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var api = value as IMusicApi;
            return api != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
