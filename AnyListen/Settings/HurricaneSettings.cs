using System;
using System.IO;
using System.Threading;
using AnyListen.Settings.Themes;

namespace AnyListen.Settings
{
    public class AnyListenSettings
    {
        private bool _isSaving;
        private static PathProvider _paths;
        private static AnyListenSettings _instance;

        public class PathProvider
        {
            public readonly string BaseDirectory;
            public readonly string CoverDirectory;

            public readonly string AccentColorsDirectory;
            public readonly string AppThemesDirectory;
            public readonly string ThemePacksDirectory;
            public readonly string AudioVisualisationsDirectory;
            // ReSharper disable once UnassignedReadonlyField
            public readonly string FFmpegPath;

            public PathProvider()
            {
                if (SaveLocationManager.IsInstalled())
                {
                    var appDataDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AnyListen"));
                    if (!appDataDir.Exists) appDataDir.Create();
                    BaseDirectory = appDataDir.FullName;
                }
                else
                {
                    BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
                CoverDirectory = Path.Combine(BaseDirectory, "AlbumCover");

                var themeDirectory = CheckDirectory(Path.Combine(BaseDirectory, "Themes"));
                AccentColorsDirectory = CheckDirectory(Path.Combine(themeDirectory, "AccentColors"));
                AppThemesDirectory = CheckDirectory(Path.Combine(themeDirectory, "AppThemes"));
                ThemePacksDirectory = CheckDirectory(Path.Combine(themeDirectory, "ThemePacks"));
                AudioVisualisationsDirectory = CheckDirectory(Path.Combine(themeDirectory, "AudioVisualisations"));
                FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            }
        }

        public static PathProvider Paths => _paths ?? (_paths = new PathProvider());

        public static AnyListenSettings Instance => _instance ?? (_instance = new AnyListenSettings());

        public PlaylistSettings Playlists { get; set; }
        public ConfigSettings Config { get; set; }
        public CurrentState CurrentState { get; set; }

        public bool IsLoaded { get; set; }

        private static string CheckDirectory(string path)
        {
            var folder = new DirectoryInfo(path);
            if (!folder.Exists) folder.Create();
            return folder.FullName;
        }

        public void Load()
        {
            ApplicationThemeManager.Instance.Refresh();
            Playlists = PlaylistSettings.Load(Paths.BaseDirectory);
            Config = ConfigSettings.Load(Paths.BaseDirectory);
            CurrentState = CurrentState.Load(Paths.BaseDirectory);
            IsLoaded = true;
        }

        public void Save()
        {
            if (_isSaving)
                return;
            _isSaving = true;
            //Important if the app gets closed
            var saveThread = new Thread(() =>
            {
                Playlists?.Save(Paths.BaseDirectory);
                Config?.Save(Paths.BaseDirectory);
                CurrentState?.Save(Paths.BaseDirectory);
                _isSaving = false;
            }) {IsBackground = false, Priority = ThreadPriority.Highest};
            saveThread.Start();
        }
    }
}
