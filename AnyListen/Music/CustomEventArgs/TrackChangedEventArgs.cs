using System;
using AnyListen.Music.Track;

namespace AnyListen.Music.CustomEventArgs
{
    public class TrackChangedEventArgs : EventArgs
    {
        public PlayableBase NewTrack { get; protected set; }

        public TrackChangedEventArgs(PlayableBase newtrack)
        {
            NewTrack = newtrack;
        }
    }
}
