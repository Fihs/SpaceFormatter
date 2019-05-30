using System.Windows;

namespace SpaceFormatter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Threading.CancellationTokenSource TokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainViewModel ViewModel => this.DataContext as MainViewModel;

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectFolder(ViewModel.FilesPath);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TokenSource = new System.Threading.CancellationTokenSource();
            ViewModel.StartFormatting(TokenSource.Token);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            TokenSource.Cancel();
        }

        private void UseRootFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilesPath = System.IO.Path.GetPathRoot(ViewModel.FilesPath);
        }
    }
}