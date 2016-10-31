using System;
using System.Windows;
using System.Xml.Serialization;

namespace AnyListen.Settings.Themes.Visual.AppThemes
{
    [Serializable]
    public class AppTheme : AppThemeBase
    {
        public override string TranslatedName => Application.Current.Resources[Name].ToString();

        public override string Group => Application.Current.Resources["Default"].ToString();

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary => new ResourceDictionary { Source = new Uri(
            $"/Resources/Themes/{Name}.xaml", UriKind.Relative) };
    }
}