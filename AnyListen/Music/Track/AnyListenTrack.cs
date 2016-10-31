using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSCore;
using CSCore.Codecs;
using AnyListen.Music.Download;
using AnyListen.Music.Track.WebApi.AnyListen;
using AnyListen.Settings;

namespace AnyListen.Music.Track
{
    public class AnyListenTrack : StreamableBase
    {
        public SongResult SongResult { get; set; }
        public override bool Equals(PlayableBase other)
        {
            if (other == null) return false;
            if (GetType() != other.GetType()) return false;
            return (SongResult.Type == ((AnyListenTrack) other).SongResult.Type) &&
                   (SongResult.SongId == ((AnyListenTrack) other).SongResult.SongId);
        }

        public async override Task<bool> LoadInformation()
        {
            return LoadInfo();
        }

        public bool LoadInfo()
        {
            Year = string.IsNullOrEmpty(SongResult.Year) ? 0 : Convert.ToUInt32(SongResult.Year.Substring(0,4));
            Title = SongResult.SongName;
            Artist = SongResult.ArtistName;
            Uploader = SongResult.AlbumArtist;
            BitRate = SongResult.BitRate;
            return true;
        }

        public override void OpenTrackLocation()
        {
            Process.Start(Link);
        }

        public override Task<IWaveSource> GetSoundSource()
        {
            return Task.Run(() => CutWaveSource(CodecFactory.Instance.GetCodec(new Uri(Link))));
        }

        protected async override Task LoadImage(DirectoryInfo albumCoverDirectory)
        {
            if (albumCoverDirectory.Exists)
            {
                var imageFile = albumCoverDirectory.GetFiles().FirstOrDefault(item => item.Name.ToLower() == (SongResult.Type + "_" + SongResult.SongId).ToLower());
                if (imageFile != null)
                {
                    Image = new BitmapImage(new Uri(imageFile.FullName));
                    return;
                }

                //Image = MusicCoverManager.GetAlbumImage(this, albumCoverDirectory);
                //if (Image != null) return;
            }

            if (AnyListenSettings.Instance.Config.LoadAlbumCoverFromInternet)
            {
                try
                {
                    if (!string.IsNullOrEmpty(SongResult.PicUrl))
                    {
                        Image = await WyMusicApi.LoadBitmapImage(this, albumCoverDirectory);
                        if (Image != null) return;
                    }
                    Image = new BitmapImage();
                }
                catch (WebException)
                {
                    //Happens, doesn't matter
                }
            }
        }

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorSoundCloud"]);
            }
            return _geometryGroup;
        }
        public override GeometryGroup ProviderVector => GetProviderVector();

        public override string Link
        {
            get
            {
                string link;
                if (!string.IsNullOrEmpty(SongResult.SqUrl))
                {
                    link = SongResult.SqUrl;
                }
                else if (!string.IsNullOrEmpty(SongResult.HqUrl))
                {
                    link = SongResult.HqUrl;
                }
                else if(!string.IsNullOrEmpty(SongResult.LqUrl))
                {
                    link = SongResult.LqUrl;
                }
                else
                {
                    link = SongResult.CopyUrl;
                }
                return CommonHelper.GetLocation(link);
            }
        }

        public override string Website => "http://www.itwusun.com";
        public override bool IsInfinityStream => false;

        public override string DownloadParameter => CommonHelper.GetDownloadUrl(SongResult, AnyListenSettings.Instance.Config.DownloadBitrate,
            AnyListenSettings.Instance.Config.LosslessPrefer, false);

        public override string DownloadFilename
        {
            get
            {
                var song = SongResult;
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
                        fileName = string.IsNullOrEmpty(song.ArtistName) ? song.AlbumName : song.ArtistName + "/" + song.AlbumName;
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