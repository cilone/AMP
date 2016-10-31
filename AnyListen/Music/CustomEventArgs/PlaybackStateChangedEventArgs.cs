using System;
using CSCore.SoundOut;

namespace AnyListen.Music.CustomEventArgs
{
    public class PlayStateChangedEventArgs : EventArgs
    {
        public PlaybackState NewPlaybackState { get; protected set; }

        public PlayStateChangedEventArgs(PlaybackState newstate)
        {
            NewPlaybackState = newstate;
        }
    }
}
