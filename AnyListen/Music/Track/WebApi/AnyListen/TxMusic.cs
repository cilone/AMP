using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class TxMusic : IMusicApi
    {
        private const string Type = "qq";
        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            if (!url.Contains("y.qq.com/"))
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
            }
            List<WebTrackResultBase> track;
            var id = Regex.Match(url, @"(?<=/)\w+(?=\.html)").Value;
            if (url.Contains("/playlist"))
            {
                track = await MusicService.MusicSearch(Type, "collect", "", id, 1, 999);
            }
            else if (url.Contains("/singer"))
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

        public string ServiceName => "腾讯音乐";

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