using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using AnyListen.Music.Track;
using AnyListen.Music.Track.WebApi.AnyListen;
using AnyListen.Utilities;
using AnyListen.ViewModelBase;
using TagLib;
using TagLib.Id3v2;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace AnyListen.Music.Download
{
    [Serializable]
    public class DownloadManager : PropertyChangedBase
    {
        [XmlIgnore]
        public ObservableCollection<DownloadEntry> Entries { get; set; }

        private bool _isOpen;
        [XmlIgnore]
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                SetProperty(value, ref _isOpen);
            }
        }

        public void AddEntry<T>(T download, DownloadSettings settings, string fileName) where T : IDownloadable, IMusicInformation
        {
            HasEntries = true;
            var entry = new DownloadEntry
            {
                IsWaiting = true,
                DownloadFilename = fileName,
                Trackname = download.WebTrack.SongName,
                DownloadParameter = download.DownloadParameter,
                DownloadMethod = download.DownloadMethod,
                MusicInformation = download,
                DownloadSettings = settings.Clone()
            };

            Entries.Add(entry);
            _hasToCheck = true;
            DownloadTracks();
        }

        private bool _isRunning;
        private bool _hasToCheck;

        private async void DownloadTracks()
        {
            while (true)
            {
                if (_isRunning) return;
                _isRunning = true;
                _hasToCheck = false;

                foreach (var entry in Entries.Where(x => !x.IsDownloaded).ToList())
                {
                    entry.IsWaiting = false;
                    var currentEntry = entry;
                    await DownloadAndConfigureTrack(entry, entry.MusicInformation, entry.DownloadFilename, d => currentEntry.Progress = d, entry.DownloadSettings);
                    entry.IsDownloaded = true;
                }

                _isRunning = false;
                if (_hasToCheck) continue;
                break;
            }
        }

        private async static Task<bool> DownloadTrack(IDownloadable download, string fileName, Action<double> progressChangedAction)
        {
            try
            {
                switch (download.DownloadMethod)
                {
                    case DownloadMethod.AnyListen:
                        await AnyListenDownloader.DownloadAnyListenTrack(download.DownloadParameter, fileName, progressChangedAction);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetExtension(IDownloadable track)
        {
            switch (track.DownloadMethod)
            {
                case DownloadMethod.AnyListen:
                    return CommonHelper.GetFormat(track.DownloadParameter);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async static Task<bool> DownloadAndConfigureTrack(IDownloadable downloadInformation, IMusicInformation musicInformation, string fileName, Action<double> progressChangedAction, DownloadSettings settings)
        {
            if (string.IsNullOrEmpty(CommonHelper.GetLocation(downloadInformation.DownloadParameter)))
            {
                return false;
            }
            if (!await DownloadTrack(downloadInformation, fileName, progressChangedAction))
            {
                return false;
            }

            if (settings.IsConverterEnabled)
            {
                var oldFile = new FileInfo(fileName);
                oldFile.MoveTo(GeneralHelper.GetFreeFileName(oldFile.Directory, oldFile.Extension).FullName); //We move the downloaded file to a temp location
                await ffmpeg.ConvertFile(oldFile.FullName, fileName, settings.Bitrate, settings.Format);
            }

            //TagLib# destroys all aac files...
            if (settings.AddTags && settings.Format != AudioFormat.AAC)
            {
                await AddTags(musicInformation, fileName);
            }
            return true;
        }

        public async static Task AddTags(IMusicInformation information, string path)
        {
            var filePath = new FileInfo(path);
            if (!filePath.Exists) return;
            try
            {
                var songResult = information.WebTrack;
                using (var file = File.Create(filePath.FullName))
                {
                    Tag.DefaultVersion = 3;
                    Tag.ForceDefaultVersion = true;
                    Tag.DefaultEncoding = StringType.UTF8;
                    if (file == null)
                    {
                        return;
                    }
                    if (filePath.FullName.Contains("ogg"))
                    {
                        var id3V1 = file.GetTag(TagTypes.Id3v1, true);
                        if (!string.IsNullOrEmpty(songResult.SongName))
                        {
                            id3V1.Title = songResult.SongName;
                        }
                        if (!string.IsNullOrEmpty(songResult.ArtistName))
                        {
                            id3V1.Performers = songResult.ArtistName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumName))
                        {
                            id3V1.Album = songResult.AlbumName;
                        }
                        if (songResult.TrackNum != 0)
                        {
                            id3V1.Track = Convert.ToUInt32(songResult.TrackNum);
                        }
                        id3V1.Disc = Convert.ToUInt32(songResult.Disc);
                        if (!string.IsNullOrEmpty(songResult.Year))
                        {
                            id3V1.Year = Convert.ToUInt32(songResult.Year.Substring(0, 4));
                        }
                        id3V1.Comment = "雅音FM";
                    }
                    else
                    {
                        TagLib.Tag tags;
                        if (filePath.FullName.Contains("ape"))
                        {
                            tags = file.GetTag(TagTypes.Ape, true);
                        }
                        else if (filePath.FullName.Contains("flac"))
                        {
                            tags = file.GetTag(TagTypes.FlacMetadata, true);
                        }
                        else
                        {
                            tags = (Tag)file.GetTag(TagTypes.Id3v2, true);
                        }
                        var picBasePath = Path.Combine(Environment.CurrentDirectory, "Pic");
                        if (!Directory.Exists(picBasePath))
                        {
                            Directory.CreateDirectory(picBasePath);
                        }
                        var picPath = Path.Combine(picBasePath, songResult.SongId + ".jpg");
                        try
                        {
                            if (!string.IsNullOrEmpty(songResult.PicUrl))
                            {
                                if (!System.IO.File.Exists(picPath))
                                {
                                    await new WebClient().DownloadFileTaskAsync(songResult.PicUrl, picPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CommonHelper.AddLog(ex.ToString());
                        }
                        try
                        {
                            if (System.IO.File.Exists(picPath))
                            {
                                var picture = new Picture(picPath)
                                {
                                    Description = "itwusun.com",
                                    MimeType = MediaTypeNames.Image.Jpeg,
                                    Type = PictureType.FrontCover
                                };
                                tags.Pictures = new IPicture[] { picture };
                            }
                        }
                        catch (Exception ex)
                        {
                            CommonHelper.AddLog(ex.ToString());
                        }
                        if (!string.IsNullOrEmpty(songResult.SongName))
                        {
                            tags.Title = songResult.SongName;
                        }
                        if (!string.IsNullOrEmpty(songResult.ArtistName))
                        {
                            tags.Performers = songResult.ArtistName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumName))
                        {
                            tags.Album = songResult.AlbumName;
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumArtist))
                        {
                            tags.AlbumArtists = songResult.AlbumArtist.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (songResult.TrackNum != 0)
                        {
                            tags.Track = Convert.ToUInt32(songResult.TrackNum);
                        }
                        tags.Disc = Convert.ToUInt32(songResult.Disc);
                        tags.Copyright = "雅音FM";
                        if (!string.IsNullOrEmpty(songResult.Year))
                        {
                            tags.Year = Convert.ToUInt32(songResult.Year.Substring(0, 4));
                            if (tags.TagTypes == TagTypes.Id3v2)
                            {
                                var dat = TextInformationFrame.Get((Tag)tags, "TDAT", true);
                                dat.Text = new[] { songResult.Year };
                            }
                        }
                        if (tags.TagTypes == TagTypes.Id3v2)
                        {
                            if (!string.IsNullOrEmpty(songResult.Company))
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TPUB", true);
                                cmp.Text = new[] { songResult.Company };
                            }
                            if (!string.IsNullOrEmpty(songResult.Language))
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TLAN", true);
                                cmp.Text = new[] { songResult.Language };
                            }
                            if (songResult.Length != 0)
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TLEN", true);
                                cmp.Text = new[] { songResult.Length.ToString() };
                            }
                            if (!string.IsNullOrEmpty(songResult.SongSubName))
                            {
                                var title = TextInformationFrame.Get((Tag)tags, "TIT3", true);
                                title.Text = new[] { songResult.SongSubName };
                            }
                        }
                        if (!string.IsNullOrEmpty(songResult.LrcUrl))
                        {
                            try
                            {
                                var html = await new WebClient { Encoding = Encoding.UTF8 }.DownloadStringTaskAsync(new Uri(songResult.LrcUrl));
                                if (!string.IsNullOrEmpty(html))
                                {
                                    html = HttpUtility.HtmlDecode(html);
                                    html = HttpUtility.HtmlDecode(html);
                                    tags.Lyrics = html;
                                }
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }
                        try
                        {
                            System.IO.File.Delete(picPath);
                        }
                        catch (Exception)
                        {
                            //
                        }
                        // ReSharper disable once AccessToDisposedClosure
                        await Task.Run(() => file.Save());
                    }
                    
                }
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
            }
        }

        public DownloadManager()
        {
            Entries = new ObservableCollection<DownloadEntry>();
            SelectedService = 0;
            Searches = new ObservableCollection<string>();
        }

        #region Settings

        private int _selectedService;
        public int SelectedService
        {
            get { return _selectedService; }
            set
            {
                SetProperty(value, ref _selectedService);
            }
        }

        public ObservableCollection<string> Searches { get; set; }


        private bool _hasEntries;
        [XmlIgnore]
        public bool HasEntries
        {
            get { return _hasEntries; }
            set
            {
                SetProperty(value, ref _hasEntries);
            }
        }

        #endregion
    }
}
