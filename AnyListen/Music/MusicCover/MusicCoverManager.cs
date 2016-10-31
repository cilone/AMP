using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AnyListen.Music.MusicCover.APIs.Lastfm;
using AnyListen.Music.Track;
using AnyListen.Settings;
using AnyListen.Utilities;

namespace AnyListen.Music.MusicCover
{
    public class MusicCoverManager
    {
        public static BitmapImage GetAlbumImage(PlayableBase track, DirectoryInfo di)
        {
            if (string.IsNullOrEmpty(track.Album)) return null;
            if (di.Exists)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in di.GetFiles("*.png"))
                {
                    if (track.Album.ToEscapedFilename().ToLower() == Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static BitmapImage GetTrackImage(PlayableBase track, DirectoryInfo di)
        {
            if (di.Exists)
            {
                var fileName = track.WebTrack == null
                    ? track.AuthenticationCode.ToString()
                    : (track.WebTrack.Type + "_" + track.WebTrack.SongId);
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in di.GetFiles("*.png"))
                {
                    if (fileName.ToLower() == Path.GetFileNameWithoutExtension(item.FullName).ToLower())
                    {
                        return new BitmapImage(new Uri(item.FullName));
                    }
                }
            }

            return null;
        }

        public static async Task<BitmapImage> LoadCoverFromWeb(PlayableBase track, DirectoryInfo di, bool useArtist = true)
        {
            var config = AnyListenSettings.Instance.Config;
            if (config.SaveCoverLocal)
            {
                if (!di.Exists) di.Create();
            }
            return await LastfmAPI.GetImage(config.DownloadAlbumCoverQuality, config.SaveCoverLocal, di, track, config.TrimTrackname, useArtist);
        }
    }
}
