using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AnyListen.Settings;
using AnyListen.Utilities;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class WyMusicApi: IMusicApi
    {
        private const string Type = "wy";
        public static async Task<BitmapImage> LoadBitmapImage(AnyListenTrack track, DirectoryInfo albumDirectory)
        {
            var config = AnyListenSettings.Instance.Config;

            using (var client = new WebClient { Proxy = null })
            {
                var image = await ImageHelper.DownloadImage(client, track.SongResult.PicUrl);
                if (config.SaveCoverLocal)
                {
                    if (!albumDirectory.Exists) albumDirectory.Create();
                    await ImageHelper.SaveImage(image, track.SongResult.Type + "_" + track.SongResult.SongId, albumDirectory.FullName);
                }
                return image;
            }
        }

        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            if (!url.Contains("music.163.com"))
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
            }
            List<WebTrackResultBase> track;
            var id = Regex.Match(url, @"(?<=id=)\d+").Value;
            if (url.Contains("/playlist"))
            {
                track = await MusicService.MusicSearch(Type, "collect", "", id, 1, 999);
            }
            else if (url.Contains("/artist"))
            {
                track = await MusicService.MusicSearch(Type, "artist", "", id, 1, 100);
            }
            else if (url.Contains("/album"))
            {
                track = await MusicService.MusicSearch(Type, "album", "", id);
            }
            else if (url.Contains("/song"))
            {
                track = await MusicService.MusicSearch(Type, "song", "", id);
            }
            else
            {
                track = null;
            }
            return track != null ? new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, track, null) : new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName => "网易云音乐";

        public override string ToString()
        {
            return ServiceName;
        }

        public bool IsEnabled => true;
        async Task<List<WebTrackResultBase>> IMusicApi.Search(string searchText)
        {
            return await MusicService.MusicSearch(Type, "search", searchText, "");
        }
        public FrameworkElement ApiSettings => null;

    }
}