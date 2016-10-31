using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

namespace AnyListen.Settings.Themes.Visual.AccentColors
{
    [Serializable]
    public class CustomAccentColor : AccentColorBase
    {
        public static bool FromFile(string filename, out CustomAccentColor result)
        {
            var accentColor = new CustomAccentColor { Name = new FileInfo(filename).Name };

            try
            {
                if (!ThemeManager.IsAccentDictionary(accentColor.ResourceDictionary))
                {
                    result = null;
                    return false;
                }
            }
            catch (XamlParseException)
            {
                result = null;
                return false;
            }

            result = accentColor;
            return true;
        }

        [XmlIgnore]
        public override string TranslatedName => Path.GetFileNameWithoutExtension(Name);

        private Brush _colorBrush;
        [XmlIgnore]
        public override Brush ColorBrush => _colorBrush ?? (_colorBrush = ResourceDictionary["AccentColorBrush"] as Brush);

        public override string Group => Application.Current.Resources["Custom"].ToString();

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary => new ResourceDictionary
        {
            Source =
                new Uri(Path.Combine(AnyListenSettings.Paths.AccentColorsDirectory, Name),
                    UriKind.RelativeOrAbsolute)
        };
    }
}
