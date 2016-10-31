using System.Windows.Media;

namespace AnyListen.PluginAPI.AudioVisualisation
{
    /// <summary>
    /// Some color information from the applied color theme
    /// </summary>
    public class ColorInformation
    {
        private Brush _accentBrush;
        private Brush _whiteBrush;
        private Brush _blackBrush;
        private Brush _grayBrush;

        /// <summary>
        /// The main color
        /// </summary>
        public Color AccentColor { get; set; }

        /// <summary>
        /// The white (= foreground) color
        /// </summary>
        public Color WhiteColor { get; set; }

        /// <summary>
        /// The black (= background) color
        /// </summary>
        public Color BlackColor { get; set; }

        /// <summary>
        /// A gray color
        /// </summary>
        public Color GrayColor { get; set; }

        /// <summary>
        /// The brush from <see cref="AccentColor"/>
        /// </summary>
        public Brush AccentBrush => _accentBrush ?? (_accentBrush = GetBrush(AccentColor));

        /// <summary>
        /// The brush from <see cref="WhiteColor"/>
        /// </summary>
        public Brush WhiteBrush => _whiteBrush ?? (_whiteBrush = GetBrush(WhiteColor));

        /// <summary>
        /// The brush from <see cref="BlackColor"/>
        /// </summary>
        public Brush BlackBrush => _blackBrush ?? (_blackBrush = GetBrush(BlackColor));

        /// <summary>
        /// The brush from <see cref="GrayColor"/>
        /// </summary>
        public Brush GrayBrush => _grayBrush ?? (_grayBrush = GetBrush(GrayColor));

        protected Brush GetBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
