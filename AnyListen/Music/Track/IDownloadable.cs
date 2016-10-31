using AnyListen.Music.Download;

namespace AnyListen.Music.Track
{
    public interface IDownloadable
    {
        string DownloadParameter { get; }
        string DownloadFilename { get; }
        DownloadMethod DownloadMethod { get; }
        bool CanDownload { get; }
    }
}
