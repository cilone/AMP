using System.IO;
using AnyListen.PluginAPI.AudioVisualisation;

namespace AnyListen.Settings.Themes.AudioVisualisation
{
    public class CustomAudioVisualisation : IAudioVisualisationContainer
    {
        public string FileName { get; set; }

        private IAudioVisualisationPlugin _loadedPlugin;
        public IAudioVisualisationPlugin Visualisation => _loadedPlugin ?? (_loadedPlugin = AudioVisualisationPluginHelper.FromFile(Path.Combine(AnyListenSettings.Paths.AudioVisualisationsDirectory, FileName)));

        public string Name => Path.GetFileNameWithoutExtension(FileName);
    }
}