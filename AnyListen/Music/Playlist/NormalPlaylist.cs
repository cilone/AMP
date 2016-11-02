using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using AnyListen.Music.CustomEventArgs;
using AnyListen.Music.Track;

namespace AnyListen.Music.Playlist
{
    [Serializable, XmlType(TypeName = "Playlist")]
    public class NormalPlaylist : PlaylistBase
    {
        private string _name;
        public override string Name
        {
            get { return _name; }
            set
            {
                SetProperty(value, ref _name);
            }
        }

        public async Task AddFiles(IEnumerable<PlayableBase> tracks)
        {
            foreach (var track in tracks)
            {
                if (!await track.LoadInformation())
                    continue;
                track.TimeAdded = DateTime.Now;
                track.IsChecked = false;
                AddTrack(track);
            }

            AsyncTrackLoader.Instance.RunAsync(this);
        }

        public async Task AddFiles(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, IEnumerable<string> paths)
        {
            var index = 0;
            var filePaths = paths as IList<string> ?? paths.ToList();
            var count = filePaths.Count();

            foreach (var fi in filePaths.Select(path => new FileInfo(path)))
            {
                if (fi.Exists)
                {
                    try
                    {
                        progresschanged?.Invoke(this, new TrackImportProgressChangedEventArgs(index, count, fi.Name));
                        var t = new LocalTrack { Path = fi.FullName };
                        if (!await t.LoadInformation()) continue;
                        t.TimeAdded = DateTime.Now;
                        t.IsChecked = false;
                        AddTrack(t);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                ++index;
            }
            AsyncTrackLoader.Instance.RunAsync(this);
        }

        public async Task AddFiles(params string[] paths)
        {
            await AddFiles(null, paths);
        }

        public async Task AddFiles(IEnumerable<string> paths)
        {
            await AddFiles(null, paths);
        }

        public async Task ReloadTrackInformation(EventHandler<TrackImportProgressChangedEventArgs> progresschanged)
        {
            foreach (var t in Tracks)
            {
                progresschanged?.Invoke(this, new TrackImportProgressChangedEventArgs(Tracks.IndexOf(t), Tracks.Count, t.ToString()));
                if (!t.TrackExists) continue;
                try
                {
                    await t.LoadInformation();
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        public override void AddTrack(PlayableBase track)
        {
            base.AddTrack(track);
            Tracks.Add(track);
            ShuffleList?.Add(track);

            track.IsAdded = true;
            var tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                track.IsAdded = false;
                tmr.Stop();
            };
            tmr.Start();
        }

        public async override void RemoveTrack(PlayableBase track)
        {
            base.RemoveTrack(track);
            ShuffleList.Remove(track);
            track.IsRemoving = true;

            await Task.Delay(500);
            if (!track.TrackExists)
            {
                for (var i = 0; i < Tracks.Count; i++)
                {
                    if (Tracks[i].AuthenticationCode != track.AuthenticationCode) continue;
                    Tracks.RemoveAt(i);
                    break;
                }
            }
            else { Tracks.Remove(track); }
            track.IsRemoving = false; //The track could be also in another playlist
        }

        public override void Clear()
        {
            Tracks.Clear();
            ShuffleList.Clear();
        }

        public override bool CanEdit => true;
    }
}
