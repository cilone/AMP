using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using CSCore;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using AnyListen.Music.CustomEventArgs;
using AnyListen.Music.Lrc;
using AnyListen.Music.MusicEqualizer;
using AnyListen.Music.Track;
using AnyListen.Music.Track.WebApi.AnyListen;
using AnyListen.Music.Visualization;
using AnyListen.PluginAPI.AudioVisualisation;
using AnyListen.Settings;
using AnyListen.ViewModelBase;

// ReSharper disable ExplicitCallerInfoArgument

// ReSharper disable InconsistentNaming

namespace AnyListen.Music.AudioEngine
{
    public class CSCoreEngine : PropertyChangedBase, IDisposable, ISpectrumProvider
    {
        private const int FFTSize = 4096;
        private const double MaxDB = 20;
        private readonly Crossfade _crossfade;
        private readonly VolumeFading _fader;

        private CancellationTokenSource _cts;
        private PlayableBase _currenttrack;
        private TimeSpan _currentTrackPosition;
        private EqualizerSettings _equalizerSettings;
        private bool _isdisposing;

        private bool _isEnabled;
        private bool _isfadingout;

        private bool _isLoading;

        private bool _manualstop;
        private bool _playAfterLoading;

        private long _position;
        private SimpleNotificationSource _simpleNotificationSource;
        private SingleBlockNotificationStream _singleBlockNotificationStream;

        private ISoundOut _soundOut;
        private float _volume = 1.0f;

        private readonly DispatcherTimer _progressHandle = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 250)
        };

        public CSCoreEngine()
        {
            _fader = new VolumeFading();
            _crossfade = new Crossfade();
            SoundOutManager = new SoundOutManager(this);
            SoundOutManager.RefreshSoundOut += (sender, args) => Refresh();
            SoundOutManager.Enable += (sender, args) => IsEnabled = true;
            SoundOutManager.Disable += (sender, args) => IsEnabled = false;
            SoundOutManager.Activate();
            _progressHandle.Tick += _progressHandle_Tick;
            if (IsEnabled)
                RefreshSoundOut();
        }

        private void _progressHandle_Tick(object sender, EventArgs e)
        {
            if (MyLrcItemList == null)
            {
                return;
            }
            var crtTime = CurrentTrackPosition.TotalSeconds;
            for (var i = 0; i < MyLrcItemList.Count; i++)
            {
                if (i != MyLrcItemList.Count - 1)
                {
                    if (!(crtTime >= MyLrcItemList[i].Time) || !(crtTime < MyLrcItemList[i + 1].Time))
                    {
                        continue;
                    }
                    if (CurrentLrcIndex != i)
                    {
                        CurrentLrcIndex = i;
                    }
                    break;
                }
                CurrentLrcIndex = MyLrcItemList.Count - 1;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fader.Dispose();
                SoundOutManager.Dispose();

                if (_soundOut != null)
                {
                    if (_fader.IsFading)
                    {
                        _fader.CancelFading();
                        _fader.WaitForCancel();
                    }
                    _soundOut.Dispose();
                    _crossfade.CancelFading();
                }
                SoundSource?.Dispose();
            }
        }

        public event EventHandler PlayStateChanged;
        public event EventHandler StartVisualization;
        public event EventHandler TrackFinished;
        public event EventHandler<TrackChangedEventArgs> TrackChanged;
        public event EventHandler<PlayStateChangedEventArgs> PlaybackStateChanged;
        public event EventHandler<PositionChangedEventArgs> PositionChanged;
        public event EventHandler VolumeChanged;
        public event EventHandler<Exception> ExceptionOccurred;
        public event EventHandler<string> SoundOutErrorOccurred;

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(value, ref _volume))
                {
                    if (_soundOut?.WaveSource != null)
                        _soundOut.Volume = value;
                    VolumeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public long Position
        {
            get
            {
                if (SoundSource == null) return 0;
                return _position;
            }
            set
            {
                if (SoundSource != null)
                {
                    SetSoundSourcePosition(value);
                    _position = value;
                }
                OnPositionChanged();
            }
        }

        public PlayableBase CurrentTrack
        {
            get { return _currenttrack; }
            protected set
            {
                if (SetProperty(value, ref _currenttrack))
                {
                    if (_currenttrack != null) OnTrackChanged();
                }
            }
        }

        public long TrackLength => SoundSource?.Length ?? 0;

        public PlaybackState CurrentState
        {
            get
            {
                if (_soundOut == null)
                {
                    return PlaybackState.Stopped;
                }
                return _soundOut.PlaybackState;
            }
        }

        public TimeSpan CurrentTrackPosition
        {
            get { return SoundSource != null ? _currentTrackPosition : TimeSpan.Zero; }
            protected set
            {
                if ((int) value.TotalSeconds != (int) _currentTrackPosition.TotalSeconds) //If the seconds changed
                    SetProperty(value, ref _currentTrackPosition);
            }
        }

        public TimeSpan CurrentTrackLength
        {
            get
            {
                try
                {
                    return SoundSource?.GetLength() ?? TimeSpan.Zero;
                }
                catch (Exception)
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public IWaveSource SoundSource { get; protected set; }

        public ConfigSettings Settings => AnyListenSettings.Instance.Config;

        public SoundOutManager SoundOutManager { get; set; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(value, ref _isEnabled); }
        }

        public Equalizer MusicEqualizer { get; set; }

        public EqualizerSettings EqualizerSettings
        {
            get { return _equalizerSettings; }
            set { if (SetProperty(value, ref _equalizerSettings)) value.EqualizerChanged += value_EqualizerChanged; }
        }

        public bool IsPlaying => (_soundOut != null && (!_isfadingout && _soundOut.PlaybackState == PlaybackState.Playing));

        protected void OnTrackFinished()
        {
            TrackFinished?.Invoke(this, EventArgs.Empty);
        }

        private string _currentLrcText;

        public string CurrentLrcText
        {
            get { return _currentLrcText; }
            set
            {
                _currentLrcText = value;
                OnPropertyChanged();
            }
        }

        private int _currentLrcIndex;

        public int CurrentLrcIndex
        {
            get { return _currentLrcIndex; }
            set
            {
                _currentLrcIndex = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MyLrcItem> _myLrcItemList;

        public ObservableCollection<MyLrcItem> MyLrcItemList
        {
            get { return _myLrcItemList; }
            set
            {
                _myLrcItemList = value;
                OnPropertyChanged();
            }
        }

        protected void OnTrackChanged()
        {
            var lrcUrl = CurrentTrack.WebTrack?.LrcUrl;
            MyLrcItemList = null;
            var tempLrcList = new ObservableCollection<MyLrcItem>();
            if (!string.IsNullOrEmpty(lrcUrl))
            {
                Task.Factory.StartNew((() =>
                {
                    var html = CommonHelper.GetHtmlContent(lrcUrl);
                    html = HttpUtility.HtmlDecode(html);
                    html = HttpUtility.HtmlDecode(html);
                    if (string.IsNullOrEmpty(html))
                    {
                        CurrentLrcIndex = 0;
                        return;
                    }
                    var lrc = new Lyric(html);
                    for (int index = 0; index < lrc.LyricTextLine.Length; index++)
                    {
                        tempLrcList.Add(new MyLrcItem
                        {
                            Time = lrc.LyricTimeLine[index],
                            LrcContent = lrc.LyricTextLine[index]
                        });
                    }
                    MyLrcItemList = tempLrcList;
                    CurrentLrcIndex = 0;
                }));
            }
            else
            {
                Task.Factory.StartNew((() =>
                {
                    var lrcHtml = "";
                    var localTrack = CurrentTrack as LocalTrack;
                    if (localTrack != null)
                    {
                        var filePath = localTrack.TrackInformation;
                        if (filePath.Exists)
                        {
                            using (var file = TagLib.File.Create(filePath.FullName))
                            {
                                lrcHtml = file.Tag.Lyrics;
                                if (string.IsNullOrEmpty(lrcHtml))
                                {
                                    var lrcPath =
                                        localTrack.Path.Substring(0, localTrack.Path.LastIndexOf('.')).TrimEnd('.') +
                                        ".lrc";
                                    if (File.Exists(lrcPath))
                                    {
                                        lrcHtml = File.ReadAllText(lrcPath);
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(lrcHtml))
                    {
                        var length = Convert.ToInt32(CurrentTrack.DurationTimespan.TotalMilliseconds);
                        var artist = CurrentTrack.Artist;
                        var songName = CurrentTrack.Title;
                        lrcHtml = CommonHelper.GetLrc(songName, artist, length);
                    }
                    if (string.IsNullOrEmpty(lrcHtml))
                    {
                        CurrentLrcIndex = 0;
                        return;
                    }
                    lrcHtml = HttpUtility.HtmlDecode(lrcHtml);
                    lrcHtml = HttpUtility.HtmlDecode(lrcHtml);
                    var lrc = new Lyric(lrcHtml);
                    for (var index = 0; index < lrc.LyricTextLine.Length; index++)
                    {
                        tempLrcList.Add(new MyLrcItem
                        {
                            Time = lrc.LyricTimeLine[index],
                            LrcContent = lrc.LyricTextLine[index]
                        });
                    }
                    MyLrcItemList = tempLrcList;
                    CurrentLrcIndex = 0;
                }));
            }
            TrackChanged?.Invoke(this, new TrackChangedEventArgs(CurrentTrack));
        }

        protected void OnSoundOutErrorOccurred(string message)
        {
            SoundOutErrorOccurred?.Invoke(this, message);
        }

        private async void SetSoundSourcePosition(long value)
        {
            try
            {
                await Task.Run(() => SoundSource.Position = value);
            }
            catch (Exception)
            {
                return;
            }

            PositionChanged?.Invoke(this, new PositionChangedEventArgs((int)CurrentTrackPosition.TotalSeconds, (int)CurrentTrackLength.TotalSeconds));
        }

        private void value_EqualizerChanged(object sender, EqualizerChangedEventArgs e)
        {
            SetEqualizerValue(e.EqualizerValue, e.EqualizerNumber);
        }

        protected void SetEqualizerValue(double value, int number)
        {
            if (MusicEqualizer == null) return;
            var perc = (value/100);
            var newvalue = (float) (perc*MaxDB);
            //the tag of the trackbar contains the index of the filter
            var filter = MusicEqualizer.SampleFilters[number];
            filter.AverageGainDB = newvalue;
        }

        protected void SetAllEqualizerSettings()
        {
            for (var i = 0; i < EqualizerSettings.Bands.Count; i++)
            {
                SetEqualizerValue(EqualizerSettings.Bands[i].Value, i);
            }
        }

        private CancellationTokenSource _trackLoadCancle;
        public async Task<bool> OpenTrack(PlayableBase track)
        {
            _trackLoadCancle?.Cancel();
            if (!IsEnabled)
            {
                OnSoundOutErrorOccurred(Application.Current.Resources["NoSoundOutDeviceFound"].ToString());
                return false;
            }
            _playAfterLoading = false;
            IsLoading = true;
            StopPlayback();
            if (CurrentTrack != null)
            {
                CurrentTrack.IsOpened = false;
                CurrentTrack.Unload();
            }
            if (SoundSource != null && !_crossfade.IsCrossfading)
            {
                SoundSource.Dispose();
            }
            track.IsOpened = true;
            CurrentTrack = track;
            _trackLoadCancle = new CancellationTokenSource();
            var t = Task.Run(() => track.Load(), _trackLoadCancle.Token);
            Equalizer equalizer;
            var result = await SetSoundSource(track);
            switch (result.State)
            {
                case State.False:
                    track.IsOpened = false;
                    return false;
                case State.Exception:
                    track.IsOpened = false;
                    IsLoading = false;
                    CurrentTrack = null;
                    ExceptionOccurred?.Invoke(this, (Exception) result.CustomState);
                    StopPlayback();
                    return false;
            }

            if (SoundSource != null && (Settings.SampleRate == -1 && SoundSource.WaveFormat.SampleRate < 44100))
            {
                SoundSource = SoundSource.ChangeSampleRate(44100);
            }
            else if (Settings.SampleRate > -1)
            {
                SoundSource = SoundSource.ChangeSampleRate(Settings.SampleRate);
            }

            SoundSource = SoundSource
                .AppendSource(x => Equalizer.Create10BandEqualizer(x.ToSampleSource()), out equalizer)
                .AppendSource(x => new SingleBlockNotificationStream(x), out _singleBlockNotificationStream)
                .AppendSource(x => new SimpleNotificationSource(x) {Interval = 100}, out _simpleNotificationSource)
                .ToWaveSource(Settings.WaveSourceBits);

            MusicEqualizer = equalizer;
            SetAllEqualizerSettings();
            _simpleNotificationSource.BlockRead += notifysource_BlockRead;
            _singleBlockNotificationStream.SingleBlockRead += notificationSource_SingleBlockRead;

            _analyser = new SampleAnalyser(FFTSize);
            _analyser.Initialize(SoundSource.WaveFormat);

            try
            {
                _soundOut.Initialize(SoundSource);
            }
            catch (Exception ex)
            {
                track.IsOpened = false;
                IsLoading = false;
                OnSoundOutErrorOccurred(ex.Message);
                return false;
            }

            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");

            CurrentStateChanged();
            _soundOut.Volume = Volume;
            StartVisualization?.Invoke(this, EventArgs.Empty);
            track.LastTimePlayed = DateTime.Now;
            if (_crossfade.IsCrossfading)
                _fader.CrossfadeIn(_soundOut, Volume);
            IsLoading = false;
            if (_playAfterLoading) TogglePlayPause();
            //await t;
            return true;
        }

        private async Task<Result> SetSoundSource(PlayableBase track)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            IWaveSource result;

            try
            {
                result = await track.GetSoundSource();
                if (token.IsCancellationRequested)
                {
                    result.Dispose();
                    return new Result(State.False);
                }
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                {
                    return new Result(State.False);
                }
                return new Result(State.Exception, ex);
            }

            SoundSource = result;
            return new Result(State.True);
        }

        public void StopPlayback()
        {
            if (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused)
            {
                _manualstop = true;
                _soundOut.Stop();
                CurrentStateChanged();
            }
        }

        public void KickTrack()
        {
            if (CurrentTrack != null)
            {
                CurrentTrack.Unload();
                CurrentTrack = null;
                SoundSource = null;
            }

            OnPropertyChanged("TrackLength");
            OnPropertyChanged("CurrentTrackLength");
            OnPositionChanged();
            CurrentStateChanged();
        }

        public async void TogglePlayPause()
        {
            try
            {
                if (IsLoading)
                {
                    _playAfterLoading = !_playAfterLoading;
                    return;
                }

                if (CurrentTrack == null) return;
                if (_fader != null && _fader.IsFading)
                {
                    _fader.CancelFading();
                    _fader.WaitForCancel();
                }
                if (_soundOut.PlaybackState == PlaybackState.Playing)
                {
                    if (_crossfade != null && _crossfade.IsCrossfading)
                    {
                        _crossfade.CancelFading();
                    }
                    _isfadingout = true;
                    CurrentStateChanged();
                    await _fader.FadeOut(_soundOut, Volume);
                    _soundOut.Pause();
                    CurrentStateChanged();
                    _isfadingout = false;
                }
                else
                {
                    try
                    {
                        _soundOut.Play();
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    CurrentStateChanged();
                    await _fader.FadeIn(_soundOut, Volume);
                }
            }
            catch (ObjectDisposedException)
            {
                //Nearly everywhere in this code block can an ObjectDisposedException get thrown. We can safely ignore that
            }
        }

        protected void OnPositionChanged()
        {
            CurrentTrackPosition = TimeSpan.FromMilliseconds(SoundSource?.WaveFormat.BytesToMilliseconds(Position) ?? 0);
            OnPropertyChanged("Position");
        }

        protected void CurrentStateChanged()
        {
            if (IsPlaying)
            {
                _progressHandle.Start();
            }
            else
            {
                _progressHandle.Stop();
            }
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("CurrentState");
            PlayStateChanged?.Invoke(this, EventArgs.Empty);
            PlaybackStateChanged?.Invoke(this, new PlayStateChangedEventArgs(CurrentState));
        }

        private void notificationSource_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            _analyser?.Add(e.Left, e.Right);
        }

        private void notifysource_BlockRead(object sender, EventArgs e)
        {
            _position = SoundSource.Position;
            OnPositionChanged();

            var seconds = (int) CurrentTrackPosition.TotalSeconds;
            var totalseconds = (int) CurrentTrackLength.TotalSeconds;
            if (PositionChanged != null)
                Application.Current.Dispatcher.Invoke(
                    () => PositionChanged(this, new PositionChangedEventArgs(seconds, totalseconds)));

            if (Settings.IsCrossfadeEnabled && totalseconds - Settings.CrossfadeDuration > 6 &&
                !_crossfade.IsCrossfading && totalseconds - seconds < Settings.CrossfadeDuration)
            {
                _fader.OutDuration = totalseconds - seconds;
                _crossfade.FadeOut(Settings.CrossfadeDuration, _soundOut);
                _simpleNotificationSource.BlockRead -= notifysource_BlockRead;
                _singleBlockNotificationStream.SingleBlockRead -= notificationSource_SingleBlockRead;
                _soundOut.Stopped -= soundOut_Stopped;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshSoundOut();
                    OnTrackFinished();
                });
            }
        }

        public void RefreshSoundOut()
        {
            _soundOut = SoundOutManager.GetNewSoundSource();
            _soundOut.Stopped += soundOut_Stopped;
        }

        public async void Refresh()
        {
            var position = Position;
            var isplaying = IsPlaying;
            if (_soundOut != null)
            {
                StopPlayback();
                _soundOut.Dispose();
            }
            SoundSource?.Dispose();
            RefreshSoundOut();
            if (CurrentTrack != null)
            {
                if (await OpenTrack(CurrentTrack))
                {
                    Position = position;
                    if (isplaying) TogglePlayPause();
                }
            }
        }

        private void soundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (_isdisposing) return;
            if (_manualstop)
            {
                _manualstop = false;
                return;
            }
            if (!e.HasError) OnTrackFinished();
            CurrentStateChanged();
        }

        private SampleAnalyser _analyser;

        public bool GetFFTData(float[] fftDataBuffer)
        {
            _analyser.CalculateFFT(fftDataBuffer);
            return IsPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double f;
            if (SoundSource != null)
            {
                f = SoundSource.WaveFormat.SampleRate/2.0;
            }
            else
            {
                f = 22050; //44100 / 2
            }
            return Convert.ToInt32((frequency/f)*(FFTSize/2));
        }
    }
}