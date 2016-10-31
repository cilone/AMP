using AnyListen.Music.Playlist;
using AnyListen.Music.Track;

namespace AnyListen.Music.Data
{
    public class TrackPlaylistPair
    {
        public PlayableBase Track { get; set; }
        public IPlaylist Playlist { get; set; }

        public TrackPlaylistPair(PlayableBase track, IPlaylist playlist)
        {
            Track = track;
            Playlist = playlist;
        }
    }
}
