using AnyListen.Designer.Data;
using AnyListen.ViewModelBase;

namespace AnyListen.Designer
{
    public class ThemePackViewModel : PropertyChangedBase
    {
        #region "Singleton & Constructor"

        private static ThemePackViewModel _instance;
        public static ThemePackViewModel Instance => _instance ?? (_instance = new ThemePackViewModel());


        private ThemePackViewModel()
        {
        }

        #endregion

        private ThemePack _themePack;
        public ThemePack ThemePack
        {
            get { return _themePack; }
            set
            {
                SetProperty(value, ref _themePack);
            }
        }
    }
}
