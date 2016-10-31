using System;
using System.IO;
using AnyListen.Utilities;
using AnyListen.ViewModelBase;

namespace AnyListen.Settings.Themes.Background
{
    public class CustomApplicationBackground : PropertyChangedBase, IApplicationBackground
    {
        protected bool Equals(CustomApplicationBackground other)
        {
            if (other.BackgroundPath == null && BackgroundPath == null) return true;
            return string.Equals(BackgroundPath, other.BackgroundPath);
        }

        public override int GetHashCode()
        {
            return (BackgroundPath != null ? BackgroundPath.GetHashCode() : 0);
        }

        private string _backgroundPath;
        public string BackgroundPath
        {
            get { return _backgroundPath; }
            set
            {
                SetProperty(value, ref _backgroundPath);
            }
        }

        public Uri GetBackground()
        {
            return new Uri(BackgroundPath);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = obj as CustomApplicationBackground;
            return Equals(other);
        }

        public bool IsAnimated => GeneralHelper.IsVideo(BackgroundPath);

        public bool IsAvailable => File.Exists(BackgroundPath);


        public string DisplayText => BackgroundPath;
    }

    public enum TextColor
    {
        Black,
        White,
        Normal
    }
}