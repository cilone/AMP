using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnyListen.Music.Download;
using AnyListen.Music.Playlist;
using AnyListen.Music.Track.WebApi.AnyListen;
using AnyListen.Settings;
using AnyListen.ViewModelBase;
using AnyListen.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace AnyListen.Music.Track.WebApi
{
    public class TrackSearcher : PropertyChangedBase
    {
        public string SearchText { get; set; }
        public ObservableCollection<WebTrackResultBase> Results { get; set; }

        private readonly AutoResetEvent _cancelWaiter;
        private bool _isSearching; //Difference between _IsRunning and IsSearching: _IsRunning is also true if pictures are downloading

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(value, ref _isLoading);
            }
        }

        private bool _nothingFound;
        public bool NothingFound
        {
            get { return _nothingFound; }
            set
            {
                SetProperty(value, ref _nothingFound);
            }
        }

        private WebTrackResultBase _selectedTrack;
        public WebTrackResultBase SelectedTrack
        {
            get { return _selectedTrack; }
            set
            {
                SetProperty(value, ref _selectedTrack);
            }
        }

        private IList _selectedTrackList;
        public IList SelectedTrackList
        {
            get { return _selectedTrackList; }
            set
            {
                SetProperty(value, ref _selectedTrackList);
            }
        }

        private IPlaylistResult _playlistResult;
        public IPlaylistResult PlaylistResult
        {
            get { return _playlistResult; }
            set
            {
                SetProperty(value, ref _playlistResult);
            }
        }

        public List<IMusicApi> MusicApis { get; set; }

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(async parameter =>
                {
                    if (string.IsNullOrWhiteSpace(SearchText)) return;
                    IsLoading = true;
                    if (_isSearching)
                    {
                        return;
                    }

                    _isSearching = true;
                    PlaylistResult = null;
                    try
                    {
                        await Search();
                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    IsLoading = false;
                    _isSearching = false;
                }));
            }
        }

        private async Task Search(bool isAlbum)
        {
            List<WebTrackResultBase> list;
            var song = SelectedTrack;
            if (song == null)
            {
                return;
            }
            if (isAlbum)
            {
                if (string.IsNullOrEmpty(song.WebTrack.AlbumId) || song.WebTrack.AlbumId == "0")
                {
                    return;
                }
                list = await MusicService.MusicSearch(song.WebTrack.Type, "album", "", song.WebTrack.AlbumId);
            }
            else
            {
                if (string.IsNullOrEmpty(song.WebTrack.ArtistId))
                {
                    return;
                }
                list = await MusicService.MusicSearch(song.WebTrack.Type, "artist", "", song.WebTrack.ArtistId, 1, 500);
            }
            NothingFound = list.Count == 0;
            SortResults(list);
        }

        private async Task Search()
        {
            foreach (var musicApi in MusicApis.Where(x => x.IsEnabled))
            {
                var result = await musicApi.CheckForSpecialUrl(SearchText);
                if (result.Item1)
                {
                    SortResults(result.Item2);
                    PlaylistResult = result.Item3;
                    return;
                }
            }
            var list = new List<WebTrackResultBase>();

            var tasks = MusicApis.Where((t, i) => t.IsEnabled && (_manager.DownloadManager.SelectedService == 0 || _manager.DownloadManager.SelectedService == i + 1)).Select(t => t.Search(SearchText)).ToList();
            foreach (var task in tasks)
            {
                var results = await task;
                if (results != null)
                {
                    list.AddRange(results);
                }
            }

            NothingFound = list.Count == 0;
            SortResults(list);
            var str = _manager.DownloadManager.Searches.FirstOrDefault(x => x.ToUpper() == SearchText.ToUpper());
            if (!string.IsNullOrEmpty(str))
            {
                _manager.DownloadManager.Searches.Move(_manager.DownloadManager.Searches.IndexOf(str), 0);
            }
            else
            {
                _manager.DownloadManager.Searches.Insert(0, SearchText);
            }
        }

        private RelayCommand _playSelectedTrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                return _playSelectedTrack ?? (_playSelectedTrack = new RelayCommand(async parameter =>
                {
                    if (SelectedTrack == null) return;
                    IsLoading = true;
                    await _manager.CSCoreEngine.OpenTrack(SelectedTrack.ToPlayable());
                    IsLoading = false;
                    if (!_manager.CSCoreEngine.IsPlaying)
                    {
                        _manager.CSCoreEngine.TogglePlayPause();
                    }
                    
                }));
            }
        }

        private RelayCommand _addToPlaylist;
        public RelayCommand AddToPlaylist
        {
            get
            {
                return _addToPlaylist ?? (_addToPlaylist = new RelayCommand(async parameter =>
                {
                    if (parameter == null) return;
                    var playlist = parameter as IPlaylist;
                    IsLoading = true;
                    if (playlist == null)
                    {
                        string result = await _baseWindow.WindowDialogService.ShowInputDialog(Application.Current.Resources["NewPlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Create"].ToString(), "", DialogMode.Single);
                        if (string.IsNullOrEmpty(result))
                        {
                            IsLoading = false;
                            return;
                        }
                        var newPlaylist = new NormalPlaylist() { Name = result };
                        _manager.Playlists.Add(newPlaylist);
                        _manager.RegisterPlaylist(newPlaylist);
                        playlist = newPlaylist;
                    }
                    PlayableBase track = new AnyListenTrack();
                    foreach (var webTrack in SelectedTrackList)
                    {
                        var selectTrack = webTrack as WebTrackResultBase;
                        if (selectTrack == null)
                        {
                            continue;
                        }
                        track = selectTrack.ToPlayable();
                        playlist.AddTrack(track);
                    }
                    IsLoading = false;
                    MainViewModel.Instance.MainTabControlIndex = 0;
                    _manager.SelectedPlaylist = playlist;
                    _manager.SelectedTrack = track;
                    _manager.SaveToSettings();
                    AsyncTrackLoader.Instance.RunAsync(playlist);
                    AnyListenSettings.Instance.Save();
                }));
            }
        }

        private RelayCommand _searchArtist;
        public RelayCommand SearchArtist
        {
            get
            {
                return _searchArtist ?? (_searchArtist = new RelayCommand(async parameter =>
                {
                    IsLoading = true;
                    if (_isSearching)
                    {
                        return;
                    }

                    _isSearching = true;
                    PlaylistResult = null;
                    try
                    {
                        await Search(false);
                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    IsLoading = false;
                    _isSearching = false;
                }));
            }
        }

        private RelayCommand _searchAlbum;
        public RelayCommand SearchAlbum
        {
            get
            {
                return _searchAlbum ?? (_searchAlbum = new RelayCommand(async parameter =>
                {
                    IsLoading = true;
                    if (_isSearching)
                    {
                        return;
                    }

                    _isSearching = true;
                    PlaylistResult = null;
                    try
                    {
                        await Search(true);
                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    IsLoading = false;
                    _isSearching = false;
                }));
            }
        }

        private RelayCommand _downloadTrack;
        public RelayCommand DownloadTrack
        {
            get
            {
                return _downloadTrack ?? (_downloadTrack = new RelayCommand(parameter =>
                {
                    var selectList = parameter as IList;
                    if (selectList == null) return;
                    foreach (var webTrack in selectList)
                    {
                        var track = webTrack as WebTrackResultBase;
                        if (track == null)
                        {
                            continue;
                        }
                        var fileName = Path.Combine(AnyListenSettings.Instance.Config.DownloadSettings.DownloadFolder,
                            track.DownloadFilename + DownloadManager.GetExtension(track));
                        _manager.DownloadManager.AddEntry(track, new DownloadSettings
                        {
                            AddTags = true,
                            IsConverterEnabled = false,
                            Bitrate = AudioBitrate.B320,
                            DownloadFolder = AnyListenSettings.Instance.Config.DownloadSettings.DownloadFolder
                        }, fileName);
                    }
                    _manager.DownloadManager.IsOpen = true;
                }));
            }
        }


        private RelayCommand _selectionChangeCommand;
        public RelayCommand SelectionChangeCommand
        {
            get
            {
                return _selectionChangeCommand ?? (_selectionChangeCommand = new RelayCommand(parameter =>
                {
                    var selectList = parameter as IList;
                    if (selectList == null) return;
                    SelectedTrackList = selectList;
                }));
            }
        }

        private RelayCommand _downloadAllTrack;
        public RelayCommand DownloadAllTrack
        {
            get
            {
                return _downloadAllTrack ?? (_downloadAllTrack = new RelayCommand(parameter =>
                {
                    foreach (var track in Results)
                    {
                        if (track == null) return;
                        var fileName = Path.Combine(AnyListenSettings.Instance.Config.DownloadSettings.DownloadFolder,
                            track.DownloadFilename + DownloadManager.GetExtension(track));

                        _manager.DownloadManager.AddEntry(track, new DownloadSettings
                        {
                            AddTags = true,
                            IsConverterEnabled = false,
                            Bitrate = AudioBitrate.B320,
                            DownloadFolder = AnyListenSettings.Instance.Config.DownloadSettings.DownloadFolder
                        }, fileName);

                    }
                    _manager.DownloadManager.IsOpen = true;
                }));
            }
        }

        private RelayCommand _addPlaylistToNewPlaylist;
        public RelayCommand AddPlaylistToNewPlaylist
        {
            get
            {
                return _addPlaylistToNewPlaylist ?? (_addPlaylistToNewPlaylist = new RelayCommand(async parameter =>
                {
                    if (PlaylistResult == null) return;
                    string result = await _baseWindow.WindowDialogService.ShowInputDialog(Application.Current.Resources["NewPlaylist"].ToString(), Application.Current.Resources["NameOfPlaylist"].ToString(), Application.Current.Resources["Create"].ToString(), PlaylistResult.Title, DialogMode.Single);
                    if (string.IsNullOrEmpty(result)) return;
                    var playlist = new NormalPlaylist { Name = result };
                    _manager.Playlists.Add(playlist);
                    _manager.RegisterPlaylist(playlist);

                    if (await AddTracksToPlaylist(playlist, PlaylistResult))
                    {
                        MainViewModel.Instance.MainTabControlIndex = 0;
                        _manager.SelectedPlaylist = playlist;
                    }
                }));
            }
        }

        private RelayCommand _addPlaylistToExisitingPlaylist;
        public RelayCommand AddPlaylistToExisitingPlaylist
        {
            get
            {
                return _addPlaylistToExisitingPlaylist ?? (_addPlaylistToExisitingPlaylist = new RelayCommand(async parameter =>
                {
                    if (PlaylistResult == null) return;
                    var playlist = parameter as NormalPlaylist;
                    if (playlist == null) return;
                    if (await AddTracksToPlaylist(playlist, PlaylistResult))
                    {
                        MainViewModel.Instance.MainTabControlIndex = 0;
                        _manager.SelectedPlaylist = playlist;
                    }
                }));
            }
        }

        private async Task<bool> AddTracksToPlaylist(IPlaylist playlist, IPlaylistResult result)
        {
            await Task.Delay(500);
            var controller = await _baseWindow.ShowProgressAsync(Application.Current.Resources["ImportTracks"].ToString(), string.Empty, true, new MetroDialogSettings { NegativeButtonText = Application.Current.Resources["Cancel"].ToString() });
            result.LoadingTracksProcessChanged += (s, e) =>
            {
                controller.SetMessage(string.Format(Application.Current.Resources["LoadingTracks"].ToString(), e.CurrentTrackName, e.Value, e.Maximum));
                controller.SetProgress(e.Value / e.Maximum);
            };

            var tracks = await result.GetTracks(controller);
            if (tracks == null)
            {
                await controller.CloseAsync();
                return false;
            }

            foreach (var track in tracks)
            {
                playlist.AddTrack(track);
            }
            _manager.SaveToSettings();
            AnyListenSettings.Instance.Save();
            AsyncTrackLoader.Instance.RunAsync(playlist);
            await controller.CloseAsync();
            return true;
        }

        private RelayCommand _copyUrl;
        public RelayCommand CopyUrl
        {
            get
            {
                return _copyUrl ?? (_copyUrl = new RelayCommand(parameter =>
                {
                    if (SelectedTrack?.WebTrack == null)
                    {
                        return;
                    }
                    var model = Convert.ToInt32(parameter);
                    string link;
                    switch (model)
                    {
                        case 0:
                            link = SelectedTrack.WebTrack.CopyUrl;
                            break;
                        case 1:
                            link = SelectedTrack.WebTrack.ArtistName + " - "+ SelectedTrack.WebTrack.SongName;
                            break;
                        case 2:
                            link = SelectedTrack.WebTrack.FlacUrl;
                            break;
                        case 3:
                            link = SelectedTrack.WebTrack.ApeUrl;
                            break;
                        case 4:
                            link = SelectedTrack.WebTrack.WavUrl;
                            break;
                        case 5:
                            link = SelectedTrack.WebTrack.SqUrl;
                            break;
                        case 6:
                            link = SelectedTrack.WebTrack.HqUrl;
                            break;
                        case 7:
                            link = SelectedTrack.WebTrack.LqUrl;
                            break;
                        case 8:
                            link = SelectedTrack.WebTrack.LrcUrl;
                            break;
                        case 9:
                            link = SelectedTrack.WebTrack.PicUrl;
                            break;
                        case 10:
                            link = SelectedTrack.WebTrack.MvHdUrl;
                            break;
                        case 11:
                            link = SelectedTrack.WebTrack.MvLdUrl;
                            break;
                        default:
                            link = SelectedTrack.WebTrack.CopyUrl;
                            break;
                    }
                    Clipboard.SetText(link);
                }));
            }
        }

        private void SortResults(IEnumerable<WebTrackResultBase> list)
        {
            Results.Clear();
            foreach (var track in list.OrderByDescending(x => x.Views).ToList())
            {
                Results.Add(track);
            }
        }

        private readonly MusicManager _manager;
        private readonly MainWindow _baseWindow;
        public TrackSearcher(MusicManager manager, MainWindow baseWindow)
        {
            Results = new ObservableCollection<WebTrackResultBase>();
            _cancelWaiter = new AutoResetEvent(false);
            _manager = manager;
            _baseWindow = baseWindow;
            MusicApis = new List<IMusicApi>
            {
                new WyMusicApi(),
                new TxMusic(),
                new BdMusic(),
                new XmMusic(),
                new TtMusic(),
                new KgMusic(),
                new KwMusic()
            }; 
        }
    }
}