using System;
using System.Globalization;
using System.Windows.Data;

namespace AnyListen.GUI.Converter
{
    public class MusicPlatformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value.ToString();
            switch (type)
            {
                case "wy":
                    return "网易";
                case "xm":
                    return "虾米";
                case "qq":
                    return "腾讯";
                case "tt":
                    return "天天";
                case "bd":
                    return "百度";
                case "kw":
                    return "酷我";
                case "echo":
                    return "回声";
                case "fs":
                    return "酷狗";
                case "dm":
                    return "多米";
                case "dx":
                    return "电信";
                case "mg":
                    return "咪咕";
                case "miui":
                    return "小米";
                default:
                    return "其他";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}