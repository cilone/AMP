using AnyListen.Designer.Data.ThemeData;

namespace AnyListen.Designer.Data
{
    public interface IPreviewable
    {
        AppThemeData AppThemeData { get; }
        AccentColorData AccentColorData { get; }
    }
}