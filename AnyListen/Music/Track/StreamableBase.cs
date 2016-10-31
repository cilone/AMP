using System.Diagnostics;
using System.Windows.Media;
using AnyListen.ViewModelBase;

namespace AnyListen.Music.Track
{
    public abstract class StreamableBase : PlayableBase, IDownloadable
    {
        public override TrackType TrackType => TrackType.Stream;

        public override bool TrackExists => true;

        public abstract GeometryGroup ProviderVector { get; }
        public string Uploader { get; set; }
        public abstract string Link { get; }
        public abstract string Website { get; }
        public abstract bool IsInfinityStream { get; }
        public string BitRate { get; set; }

        //IDownloadable
        public abstract string DownloadParameter { get; }
        public abstract string DownloadFilename { get; }
        public abstract Download.DownloadMethod DownloadMethod { get; }
        public abstract bool CanDownload { get; }

        // ReSharper disable once InconsistentNaming
        protected RelayCommand _openLinkCommand;
        public virtual RelayCommand OpenLinkCommand
        {
            get { return _openLinkCommand ?? (_openLinkCommand = new RelayCommand(parameter => { Process.Start(Link); })); }
        }

        // ReSharper disable once InconsistentNaming
        protected RelayCommand _openWebsiteCommand;
        public virtual RelayCommand OpenWebsiteCommand
        {
            get { return _openWebsiteCommand ?? (_openWebsiteCommand = new RelayCommand(parameter => { Process.Start(Website); })); }
        }
    }
}
