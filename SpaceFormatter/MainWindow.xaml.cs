using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SpaceFormatter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly OpenFileDialog fd;
        private readonly Random rand;
        private ulong fileSize;
        private string filesPath;
        private bool isFormatting;

        #endregion Fields

        #region Constructor

        public MainWindow()
        {
            filesPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Temp\";

            rand = new Random();

            fd = new OpenFileDialog
            {
                Title = "Select Folder",
                InitialDirectory = Path.GetPathRoot(filesPath),
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            InitializeComponent();

            PathTextBlock.Text = System.AppDomain.CurrentDomain.BaseDirectory;
        }

        #endregion Constructor

        #region Methods

        private void CreateDirectory(string path)
        {
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        private void CreateFile(string path, string name, byte[] bytes)
        {
            File.WriteAllBytes(path + name, bytes);
        }

        private void DeleteDirectory(string path)
        {
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    // Delete the directory.
                    Directory.Delete(path, true);
                    Console.WriteLine("The directory was deleted successfully.");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        private long GetDriveFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }

        private void StartFormat()
        {
            if (!ulong.TryParse(Dispatcher.Invoke(() => FileSizeTextBox.Text), out fileSize))
            {
                fileSize = 10485760;
                Dispatcher.Invoke(() => FileSizeTextBox.Text = "10485760");
            }

            Dispatcher.Invoke(() => Status.Value = 0);
            Dispatcher.Invoke(() => StatusTextBlock.Text = "Creating temp folder...");

            CreateDirectory(filesPath);

            var data = GetDriveFreeSpace(Path.GetPathRoot(filesPath));

            if (data == -1 || data == 0)
            {
                Dispatcher.Invoke(() => StatusTextBlock.Text = "Error");
                return;
            }

            var count = (data / (long)fileSize) + 1;

            var timer = new EtcCalculator((int)count);

            byte[] bytes = new byte[fileSize];

            isFormatting = true;

            for (int i = 1; i <= count; i++)
            {
                if (!isFormatting)
                    break;

                Dispatcher.Invoke(() => Status.Value = i / ((double)count / 100));
                Dispatcher.Invoke(() => StatusTextBlock.Text = $"Creating temp files... {i}\\{count}");

                data = GetDriveFreeSpace(Path.GetPathRoot(filesPath));

                if (data < bytes.Length)
                {
                    bytes = new byte[data];
                    rand.NextBytes(bytes);
                    CreateFile(filesPath, i.ToString(), bytes);
                    break;
                }

                if (i == 1 || Dispatcher.Invoke(() => RandomDataСheckBox.IsChecked ?? true))
                {
                    rand.NextBytes(bytes);
                }

                CreateFile(filesPath, i.ToString(), bytes);

                Dispatcher.Invoke(() => ETCTextBlock.Text = "Estimate time: " + timer.GetEtc(i).ToString(@"hh\:mm\:ss"));
            }

            if (Dispatcher.Invoke(() => DeleteTempCheckBox.IsChecked ?? false))
            {
                Dispatcher.Invoke(() => StatusTextBlock.Text = "Deleting temp folder...");

                DeleteDirectory(filesPath);
            }

            isFormatting = false;

            Dispatcher.Invoke(() => Status.Value = 100);
            Dispatcher.Invoke(() => StatusTextBlock.Text = "Completed!");
            Dispatcher.Invoke(() => ETCTextBlock.Text = String.Empty);
        }

        #endregion Methods

        #region Clicks

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            fd.FileName = "Select Folder";
            fd.InitialDirectory = PathTextBlock.Text;

            if (fd.ShowDialog() == true)
            {
                PathTextBlock.Text = Path.GetDirectoryName(fd.FileName);

                filesPath = PathTextBlock.Text + @"\Temp\";
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFolderButton.IsEnabled = false;
            StartButton.IsEnabled = false;
            UseRootFolderButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            RandomDataСheckBox.IsEnabled = false;
            FileSizeTextBox.IsEnabled = false;

            await Task.Run(() => StartFormat());

            SelectFolderButton.IsEnabled = true;
            StartButton.IsEnabled = true;
            UseRootFolderButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            RandomDataСheckBox.IsEnabled = true;
            FileSizeTextBox.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isFormatting = false;
        }

        private void UseRootFolderButton_Click(object sender, RoutedEventArgs e)
        {
            PathTextBlock.Text = Path.GetPathRoot(filesPath);

            filesPath = PathTextBlock.Text + @"\Temp\";
        }

        #endregion Clicks
    }
}