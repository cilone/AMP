using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class TtMusic : IMusicApi
    {
        private const string Type = "tt";
        async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> IMusicApi.CheckForSpecialUrl(string url)
        {
            if (!url.Contains("dongting.com/"))
            {
                return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
            }
            List<WebTrackResultBase> track;
            var id = Regex.Match(url, @"(?<=id=)\d+").Value;
            if (url.Contains("music_songlist"))
            {
                track = await MusicService.MusicSearch(Type, "collect", "", id, 1, 999);
            }
            else if (url.Contains("music_artist") || url.Contains("music_singer"))
            {
                track = await MusicService.MusicSearch(Type, "artist", "", id, 1, 100);
            }
            else if (url.Contains("music_album"))
            {
                track = await MusicService.MusicSearch(Type, "album", "", id);
            }
            else if (url.Contains("music_player"))
            {
                track = await MusicService.MusicSearch(Type, "song", "", id);
            }
            else
            {
                track = null;
            }
            return track != null ? new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(true, track, null) : new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName => "天天动听";

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