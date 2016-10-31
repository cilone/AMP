using AnyListen.PluginAPI.AudioVisualisation;

namespace AnyListen.Settings.Themes.AudioVisualisation
{
    public interface IAudioVisualisationContainer
    {
        IAudioVisualisationPlugin Visualisation { get; }
        string Name { get; }
    }
}
