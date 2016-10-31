using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnyListen.Settings;
using AnyListen.Settings.Themes;

namespace AnyListen.Designer.Data.ThemeData
{
    public class AccentColorData : DataThemeBase
    {
        public AccentColorData()
        {
            ThemeSettings = new List<IThemeSetting>
            {
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"LightColor\">(?<content>(.*?))<",
                    ID = "LightColor",
                    DisplayName ="Light color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"BrightColor\">(?<content>(.*?))<",
                    ID = "BrightColor",
                    DisplayName ="Bright color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"NormalColor\">(?<content>(.*?))<",
                    ID = "NormalColor",
                                        DisplayName = "Normal color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"DarkColor\">(?<content>(.*?))<",
                    ID = "DarkColor",
                    DisplayName ="Dark color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"HighlightColor\">(?<content>(.*?))<",
                    ID = "HighlightColor",
                    DisplayName ="Hightlight color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"AccentColor\">(?<content>(.*?))<",
                    ID = "AccentColor",
                    DisplayName ="Accent color",
                    IsTransparencyEnabled = false
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"AccentSelectedColorBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "AccentSelectedColorBrush",
                    DisplayName ="Selected color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"IdealForegroundColor\">(?<content>(.*?))<",
                    ID = "IdealForegroundColor",
                    DisplayName ="Ideal foreground color"
                }
            };
        }

        public static AccentColorData LoadDefault()
        {
            var accentColor = new AccentColorData();
            accentColor.LoadFromResourceDictionary(ApplicationThemeManager.Instance.AccentColors.First(x => x.Name == "Cyan").ResourceDictionary);
            return accentColor;
        }

        public override string Source => Properties.Resources.AccentColor;

        public override string Filter => $"{Application.Current.Resources["AccentColorString"]}|*.xaml";

        public override string BaseDirectory => AnyListenSettings.Paths.AccentColorsDirectory;
    }
}