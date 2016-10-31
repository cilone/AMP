using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class BdMusic : IMusicApi
    {
        private const string Type = "bd";
        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            if (!url.Contains("music.baidu.com/"))
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
            }
            List<WebTrackResultBase> track;
            var id = Regex.Match(url, @"(?<=baidu.com/\w+/)\d+").Value;
            if (url.Contains("/songlist"))
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

        public string ServiceName => "百度音乐";

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