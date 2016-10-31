using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using CSCore;
using CSCore.Codecs;
using AnyListen.Music.Download;
using System.Xml.Serialization;
using AnyListen.ViewModelBase;
using System.Windows;

namespace AnyListen.Music.Track
{
    public class CustomStream : StreamableBase
    {
        public string StreamUrl { get; set; }
        public override bool IsInfinityStream => IsInfinityStreamSerializable;

        [XmlElement("IsInfinityStream")]
        public bool IsInfinityStreamSerializable { get; set; }

        public override bool TrackExists => true;

        public async override Task<bool> LoadInformation()
        {
            return true;
        }

        public override void OpenTrackLocation()
        {
            Process.Start(StreamUrl);
        }

        public override TrackType TrackType => TrackType.Stream;

        public override Task<IWaveSource> GetSoundSource()
        {
            return Task.Run(() => CodecFactory.Instance.GetCodec(new Uri(StreamUrl)));
        }

        public override bool Equals(PlayableBase other)
        {
            return other == this;
        }

        protected async override Task LoadImage(DirectoryInfo albumCoverDirectory)
        {
            
        }

        public override string DownloadParameter
        {
            get { throw new NotImplementedException(); }
        }

        public override string DownloadFilename
        {
            get { throw new NotImplementedException(); }
        }

        public override DownloadMethod DownloadMethod
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanDownload => false;

        public override GeometryGroup ProviderVector => GetProviderVector();

        private static GeometryGroup _geometryGroup;
        public static GeometryGroup GetProviderVector()
        {
            if (_geometryGroup == null)
            {
                _geometryGroup = new GeometryGroup();
                _geometryGroup.Children.Add((Geometry)Application.Current.Resources["VectorWebsite"]);
            }
            return _geometryGroup;
        }

        public override RelayCommand OpenWebsiteCommand
        {
            get
            {
                return _openWebsiteCommand ??
                       (_openWebsiteCommand = new RelayCommand(parameter => { Process.Start("http://" + Website); }));
            }
        }

        public async override Task<bool> CheckTrack()
        {
            if (!TrackExists) return false;
            try
            {
                using (var soundSource = await GetSoundSource())
                {
                    SetDuration(soundSource.GetLength());
                    kHz = soundSource.WaveFormat.SampleRate / 1000;
                    IsInfinityStreamSerializable = soundSource.Length == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }

            IsChecked = true;
            return true;
        }

        public override string Link => StreamUrl;

        public override string Website => new Uri(StreamUrl).Host;
    }
}
