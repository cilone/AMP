using AnyListen.Music;
using AnyListen.ViewModels;

namespace AnyListen.Views
{
    /// <summary>
    /// Interaction logic for QueueManagerWindow.xaml
    /// </summary>
    public partial class QueueManagerWindow
    {
        public QueueManager QueueManager { get; set; }

        public QueueManagerWindow(QueueManager queueManager)
        {
            DataContext = new QueueManagerViewModel(queueManager);
            InitializeComponent();
        }
    }
}
