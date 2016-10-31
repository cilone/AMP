using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using CSCore.SoundOut;
using AnyListen.AppCommunication;
using AnyListen.Music.AudioEngine;
using AnyListen.Music.Download;
using AnyListen.Notification;
using AnyListen.Settings.Themes;
using MahApps.Metro.Controls;

// ReSharper disable InconsistentNaming
namespace AnyListen.Settings
{
    [Serializable, XmlType(TypeName = "Settings")]
    public class ConfigSettings : SettingsBase
    {
        public const string Filename = "config.xml";

        //CSCore
        public string SoundOutDeviceID { get; set; }
        public SoundOutMode SoundOutMode { get; set; }
        public int Latency { get; set; }
        public bool IsCrossfadeEnabled { get; set; }
        public int CrossfadeDuration { get; set; }

        //Playback
        public int WaveSourceBits { get; set; }
        public int SampleRate { get; set; }

        //Magic Arrow
        public bool ShowMagicArrowBelowCursor { get; set; }


        //Search
        public string PersonalCode { get; set; }
        public int DownloadBitrate { get; set; }
        public int LosslessPrefer { get; set; }
        public int FileNameFormat { get; set; }
        public int FileFloderFormat { get; set; }

        //Design
        public ApplicationDesign ApplicationDesign { get; set; }

        private bool _useThinHeaders;
        public bool UseThinHeaders
        {
            get { return _useThinHeaders; }
            set
            {
                SetProperty(value, ref _useThinHeaders);
            }
        }
        
        private TransitionType _tabControlTransition;
        public TransitionType TabControlTransition
        {
            get { return _tabControlTransition; }
            set
            {
                SetProperty(value, ref _tabControlTransition);
            }
        }

        //General
        public string Language { get; set; }
        public bool RememberTrackImportPlaylist { get; set; }
        public string PlaylistToImportTrack { get; set; }
        public bool ShufflePreferFavoriteTracks { get; set; }
        public bool ShowArtistAndTitle { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ShowNotificationIfMinimizeToTray { get; set; }
        public bool ShowProgressInTaskbar { get; set; }

        public List<PasswordEntry> Passwords { get; set; }

        //Notifications
        public NotificationType Notification { get; set; }
        public bool DisableNotificationInGame { get; set; }
        public int NotificationShowTime { get; set; }

        //Music
        public bool LoadAlbumCoverFromInternet { get; set; }
        public ImageQuality DownloadAlbumCoverQuality { get; set; }
        public bool SaveCoverLocal { get; set; }
        public bool TrimTrackname { get; set; }
        public DownloadManager Downloader { get; set; }
        public DownloadSettings DownloadSettings { get; set; }

        //App
        public AppCommunicationSettings AppCommunicationSettings { get; set; }
        [XmlIgnore]
        public AppCommunicationManager AppCommunicationManager { get; set; }

        //Updates
        public bool CheckForAnyListenUpdates { get; set; }
        public bool CheckForYoutubeDlUpdates { get; set; }

        private List<LanguageInfo> _languages;
        [XmlIgnore]
        public List<LanguageInfo> Languages => _languages ?? (_languages = new List<LanguageInfo>
        {
            new LanguageInfo("简体中文", "/Resources/Languages/AnyListen.zh-cn.xaml",
                new Uri("/Resources/Languages/Icons/cn.png", UriKind.Relative), "Shelher", "zh"),
            new LanguageInfo("Deutsch", "/Resources/Languages/AnyListen.de-de.xaml",
                new Uri("/Resources/Languages/Icons/de.png", UriKind.Relative), "Alkaline", "de"),
            new LanguageInfo("English", "/Resources/Languages/AnyListen.en-us.xaml",
                new Uri("/Resources/Languages/Icons/us.png", UriKind.Relative), "Alkaline", "en"),
            new LanguageInfo("Nederlands", "/Resources/Languages/AnyListen.nl-nl.xaml",
                new Uri("/Resources/Languages/Icons/nl.png", UriKind.Relative), "DrawCase", "nl"),
            new LanguageInfo("Suomi", "/Resources/Languages/AnyListen.fi-fi.xaml",
                new Uri("/Resources/Languages/Icons/fi.png", UriKind.Relative), "Väinämö Vettenranta", "fi"),
            new LanguageInfo("Russian", "/Resources/Languages/AnyListen.ru-ru.xaml",
                new Uri("/Resources/Languages/Icons/ru.png", UriKind.Relative), "Barmin Alexander", "ru")
        });

        public override sealed void SetStandardValues()
        {
            SoundOutDeviceID = SoundOutManager.DefaultDevicePlaceholder;
            DisableNotificationInGame = true;
            ShowMagicArrowBelowCursor = true;
            WaveSourceBits = 16;
            SampleRate = -1;
            var language = Languages.FirstOrDefault(x => x.Code == Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
            Language = language == null ? "zh" : language.Code;
            Notification = NotificationType.Top;
            ApplicationDesign = new ApplicationDesign();
            ApplicationDesign.SetStandard();
            NotificationShowTime = 5000;
            RememberTrackImportPlaylist = false;
            PlaylistToImportTrack = null;
            LoadAlbumCoverFromInternet = true;
            DownloadAlbumCoverQuality = ImageQuality.Maximum;
            SaveCoverLocal = true;
            TrimTrackname = true;
            ShowArtistAndTitle = true;
            SoundOutMode = WasapiOut.IsSupportedOnCurrentPlatform ? SoundOutMode.WASAPI : SoundOutMode.DirectSound;
            Latency = 100;
            IsCrossfadeEnabled = false;
            CrossfadeDuration = 4;
            UseThinHeaders = true;
            MinimizeToTray = false;
            ShowNotificationIfMinimizeToTray = true;
            Downloader = new DownloadManager();
            TabControlTransition = TransitionType.Left;
            ShowProgressInTaskbar = true;
            AppCommunicationSettings = new AppCommunicationSettings();
            AppCommunicationSettings.SetStandard();
            DownloadSettings = new DownloadSettings();
            DownloadSettings.SetDefault();
            CheckForAnyListenUpdates = true;
            CheckForYoutubeDlUpdates = true;
            Passwords = new List<PasswordEntry>();
            FileNameFormat = 1;
            DownloadBitrate = 1;
        }

        public ConfigSettings()
        {
            SetStandardValues();
        }

        public void LoadAppCommunication()
        {
            if (AppCommunicationManager == null)
                AppCommunicationManager = new AppCommunicationManager(AppCommunicationSettings);
            if (AppCommunicationSettings.IsEnabled) AppCommunicationManager.Start();
        }

        private ResourceDictionary _lastLanguage;
        public void LoadLanguage()
        {
            if (_lastLanguage != null) Application.Current.Resources.MergedDictionaries.Remove(_lastLanguage);
            _lastLanguage = new ResourceDictionary { Source = new Uri(Languages.First(x => x.Code == Language).Path, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(_lastLanguage);
        }

        public override void Save(string programPath)
        {
            Save<ConfigSettings>(Path.Combine(programPath, Filename));
        }

        public static ConfigSettings Load(string programpath)
        {
            var fi = new FileInfo(Path.Combine(programpath, Filename));
            ConfigSettings result;
            if (!fi.Exists || string.IsNullOrWhiteSpace(File.ReadAllText(fi.FullName)))
            {
                result = new ConfigSettings();
            }
            else
            {
                using (var reader = new StreamReader(Path.Combine(programpath, Filename)))
                {
                    var deserializer = new XmlSerializer(typeof(ConfigSettings));
                    result = (ConfigSettings)deserializer.Deserialize(reader);
                }
            }
            result.LoadLanguage();
            result.LoadAppCommunication();
            return result;
        }

        protected bool CompareTwoValues(object v1, object v2)
        {
            if (v1 == null || v2 == null) return false;
            return v1.Equals(v2);
        }
    }

    public enum ImageQuality
    {
        Small, Medium, Large, Maximum
    }

    public enum SoundOutMode
    {
        DirectSound, WASAPI
    }
}
