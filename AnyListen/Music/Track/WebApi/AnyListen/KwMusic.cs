using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class KwMusic : IMusicApi
    {
        private const string Type = "kw";
        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            if (!url.Contains("kuwo.cn"))
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
            }
            List<WebTrackResultBase> track;
            var id = "";
            if (url.Contains("cinfo"))
            {
                id = Regex.Match(url, @"(?<=cinfo_)\d+").Value;
                track = await MusicService.MusicSearch(Type, "collect", "", id, 1, 999);
            }
            else if (url.Contains("mingxing"))
            {
                track = null;
            }
            else if (url.Contains("album"))
            {
                id = Regex.Match(url, @"(?<=album/)\d+").Value;
                track = await MusicService.MusicSearch(Type, "album", "", id);
            }
            else if (url.Contains("yinyue"))
            {
                id = Regex.Match(url, @"(?<=yinyue/)\d+").Value;
                track = await MusicService.MusicSearch(Type, "song", "", id);
            }
            else
            {
                track = null;
            }
            return track != null ? new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, track, null) : new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName => "酷我音乐";

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