using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using CSCore.Codecs.FLAC;
using AnyListen.Music.Download;
using AnyListen.Settings;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class AnyListenWebResult : WebTrackResultBase
    {
        public override ProviderName ProviderName => ProviderName.AnyListen;
        public override PlayableBase ToPlayable()
        {
            var result = (SongResult)Result;
            var newtrack = new AnyListenTrack
            {
                SongResult = result,
                TimeAdded = DateTime.Now,
                IsChecked = false,
                Artist = result.ArtistName,
                Album = result.AlbumName,
                Duration = CommonHelper.NumToTime(result.Length),
                WebTrack = result
            };
            newtrack.LoadInformation();
            return newtrack;
        }

        public override GeometryGroup ProviderVector => AnyListenTrack.GetProviderVector();

        public override string DownloadParameter
        {
            get
            {
                var song = (SongResult) Result;
                return CommonHelper.GetDownloadUrl(song, AnyListenSettings.Instance.Config.DownloadBitrate,
                    AnyListenSettings.Instance.Config.LosslessPrefer, false);
            }
        }

        public override string DownloadFilename
        {
            get
            {
                var song = (SongResult)Result;
                song.ArtistName = CommonHelper.RemoveSpicalChar(song.ArtistName);
                song.AlbumName = CommonHelper.RemoveSpicalChar(song.AlbumName);
                song.SongName = CommonHelper.RemoveSpicalChar(song.SongName);
                var fileName = "";
                switch (AnyListenSettings.Instance.Config.FileFloderFormat)
                {
                    case 0:
                        fileName = "";
                        break;
                    case 1:
                        fileName = song.ArtistName;
                        break;
                    case 2:
                        fileName = song.AlbumName;
                        break;
                    case 3:
                        fileName = string.IsNullOrEmpty(song.ArtistName) ? song.AlbumName: song.ArtistName + "/" + song.AlbumName;
                        break;
                }
                fileName = string.IsNullOrEmpty(fileName.TrimEnd('/')) ? "" : fileName.TrimEnd('/') + "/";
                switch (AnyListenSettings.Instance.Config.FileNameFormat)
                {
                    case 0:
                        fileName += song.SongName;
                        break;
                    case 1:
                        fileName += (string.IsNullOrEmpty(song.ArtistName) ? song.SongName : (song.ArtistName + " - " + song.SongName));
                        break;
                    case 2:
                        fileName += (string.IsNullOrEmpty(song.ArtistName) ? song.SongName : (song.SongName + " - " + song.ArtistName));
                        break;
                    case 3:
                        fileName += ((song.TrackNum == 0)
                            ? song.SongName
                            : ((song.TrackNum >= 100
                                ? song.TrackNum.ToString()
                                : song.TrackNum.ToString().PadLeft(2, '0')) + " - " + song.SongName));
                        break;
                }
                return fileName;
            }
        }
        public override DownloadMethod DownloadMethod => DownloadMethod.AnyListen;
        public override bool CanDownload => true;
    }
}