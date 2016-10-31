using System;
using System.Windows.Media;

namespace AnyListen.Designer.Data
{
    public class ThemeColor : IThemeSetting
    {
        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        public string RegexPattern { get; set; }
        public bool IsTransparencyEnabled { get; set; }

        public ThemeColor()
        {
            Color = Color.FromArgb(255, 0, 0, 0);
            IsTransparencyEnabled = true;
        }

        public string DisplayName { get; set; }

        public string ID { get; set; }

        public string Value => ColorToString(Color, IsTransparencyEnabled);

        private string ColorToString(Color c, bool withTransparencyValue = true)
        {
            return withTransparencyValue ? $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}"
                : $"{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        public void SetValue(string content)
        {
            var newColor = ColorConverter.ConvertFromString(content);
            if (newColor == null) return;
            Color = (Color)newColor;
        }

        public event EventHandler ValueChanged;
    }
}