﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using AnyListen.Designer.Data;
using AnyListen.Settings.Themes.AudioVisualisation;
using AnyListen.Settings.Themes.Visual.AccentColors;
using AnyListen.Settings.Themes.Visual.AppThemes;
using MahApps.Metro;
using AppTheme = AnyListen.Settings.Themes.Visual.AppThemes.AppTheme;

namespace AnyListen.Settings.Themes
{
    public class ApplicationThemeManager
    {
        #region "Singleton & Constructor"

        private static ApplicationThemeManager _instance;
        public static ApplicationThemeManager Instance => _instance ?? (_instance = new ApplicationThemeManager());


        private ApplicationThemeManager()
        {
            _loadedResources = new Dictionary<string, ResourceDictionary>();
        }

        #endregion

        private ObservableCollection<AccentColorBase> _accentColors;
        public ObservableCollection<AccentColorBase> AccentColors => _accentColors;

        private ObservableCollection<AppThemeBase> _appThemes;
        public ObservableCollection<AppThemeBase> AppThemes => _appThemes;

        private ObservableCollection<ThemePack> _themePacks;
        public ObservableCollection<ThemePack> ThemePacks => _themePacks;

        private ObservableCollection<IAudioVisualisationContainer> _audioVisualisations;
        public ObservableCollection<IAudioVisualisationContainer> AudioVisualisations => _audioVisualisations;

        public void Refresh()
        {
            _accentColors = new ObservableCollection<AccentColorBase>();
            _appThemes = new ObservableCollection<AppThemeBase>();
            _themePacks = new ObservableCollection<ThemePack>();
            _audioVisualisations = new ObservableCollection<IAudioVisualisationContainer>();

            foreach (var t in ThemeManager.Accents.Select(a => new AccentColor { Name = a.Name }).OrderBy(x => x.TranslatedName))
            {
                _accentColors.Add(t);
            }

            foreach (var t in ThemeManager.AppThemes.Select(a => new AppTheme { Name = a.Name }).OrderBy(x => x.TranslatedName))
            {
                _appThemes.Add(t);
            }

            foreach (var defaultAudioVisualisation in ApplicationDesign.DefaultAudioVisualisations)
            {
                _audioVisualisations.Add(defaultAudioVisualisation);
            }

            var accentColorsFolder = new DirectoryInfo(AnyListenSettings.Paths.AccentColorsDirectory);
            if (accentColorsFolder.Exists)
            {
                foreach (var file in accentColorsFolder.GetFiles("*.xaml"))
                {
                    CustomAccentColor theme;

                    if (CustomAccentColor.FromFile(file.FullName, out theme))
                        _accentColors.Add(theme);
                }
            }

            var appThemesFolder = new DirectoryInfo(AnyListenSettings.Paths.AppThemesDirectory);
            if (appThemesFolder.Exists)
            {
                foreach (var file in appThemesFolder.GetFiles("*.xaml"))
                {
                    CustomAppTheme theme;
                    if (CustomAppTheme.FromFile(file.FullName, out theme))
                        _appThemes.Add(theme);
                }
            }

            var themePacksFolder = new DirectoryInfo(AnyListenSettings.Paths.ThemePacksDirectory);
            if (themePacksFolder.Exists)
            {
                foreach (var file in themePacksFolder.GetFiles("*.htp"))
                {
                    ThemePack pack;
                    if (ThemePack.FromFile(file.FullName, out pack))
                    {
                        _themePacks.Add(pack);
                    }
                }
            }

            var audioVisualisations = new DirectoryInfo(AnyListenSettings.Paths.AudioVisualisationsDirectory);
            if (audioVisualisations.Exists)
            {
                foreach (var file in audioVisualisations.GetFiles("*.dll"))
                {
                    _audioVisualisations.Add(new CustomAudioVisualisation { FileName = file.Name });
                }
            }
        }

        public ThemePack GetThemePack(string name)
        {
            return null;
        }

        public void Apply(ApplicationDesign design)
        {
            try
            {
                design.AccentColor.ApplyTheme();
            }
            catch (Exception)
            {
                design.AccentColor = AccentColors.First(x => x.Name == "Blue");
                design.AccentColor.ApplyTheme();
            }

            try
            {
                design.AppTheme.ApplyTheme();
            }
            catch (Exception)
            {
                design.AppTheme = AppThemes.First();
                design.AppTheme.ApplyTheme();
            }

            if (design.AudioVisualisation != null)
                design.AudioVisualisation.Visualisation.Refresh();
        }

        private readonly Dictionary<string, ResourceDictionary> _loadedResources;
        public void LoadResource(string key, ResourceDictionary resource)
        {
            Application.Current.Resources.MergedDictionaries.Add(resource);

            if (_loadedResources.ContainsKey(key))
            {
                Application.Current.Resources.MergedDictionaries.Remove(_loadedResources[key]);
                _loadedResources.Remove(key);
            }

            _loadedResources.Add(key, resource);
        }
    }
}