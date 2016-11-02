using System;
using System.ComponentModel;
using System.Windows;
using AnyListen.Settings;

namespace AnyListen.Views
{
    /// <summary>
    /// Interaction logic for ReportExceptionWindow.xaml
    /// </summary>
    public partial class ReportExceptionWindow : INotifyPropertyChanged
    {
        public ReportExceptionWindow(Exception error)
        {
            InitializeComponent();
            Error = error;
            if (AnyListenSettings.Instance.IsLoaded) AnyListenSettings.Instance.Save();
        }

        private Exception _error;
        public Exception Error
        {
            get { return _error; }
            set { _error = value; OnPropertyChanged("Error"); }
        }

        private void ButtonSendErrorReport_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
