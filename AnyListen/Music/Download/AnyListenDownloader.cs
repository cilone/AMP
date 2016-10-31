using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AnyListen.Music.Track.WebApi.AnyListen;

namespace AnyListen.Music.Download
{
    public class AnyListenDownloader
    {
        public static async Task DownloadAnyListenTrack(string link, string fileName, Action<double> progressChangedAction)
        {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            using (var client = new WebClient { Proxy = null})
            {
                client.DownloadProgressChanged += (s, e) => progressChangedAction.Invoke(e.ProgressPercentage);
                await client.DownloadFileTaskAsync(link, fileName);
            }
        }
    }
}