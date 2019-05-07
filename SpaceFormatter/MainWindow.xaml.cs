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
        private ulong fileSize;

        private readonly OpenFileDialog fd;

        private readonly Random rand;

        private string FilesPath;

        private bool IsFormatting;

        public MainWindow()
        {
            FilesPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Temp\";

            rand = new Random();

            fd = new OpenFileDialog
            {
                Title = "Select Folder",
                InitialDirectory = Path.GetPathRoot(FilesPath),
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            InitializeComponent();
        }

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

            CreateDirectory(FilesPath);

            var data = GetDriveFreeSpace(Path.GetPathRoot(FilesPath));

            if (data == -1 || data == 0)
            {
                Dispatcher.Invoke(() => StatusTextBlock.Text = "Error");
                return;
            }

            var count = (data / (long)fileSize) + 1;

            var timer = new EtcCalculator((int)count);

            byte[] bytes = new byte[fileSize];

            IsFormatting = true;

            for (int i = 1; i <= count; i++)
            {
                if (!IsFormatting)
                    break;

                Dispatcher.Invoke(() => Status.Value = i / ((double)count / 100));
                Dispatcher.Invoke(() => StatusTextBlock.Text = $"Creating temp files... {i}\\{count}");

                data = GetDriveFreeSpace(Path.GetPathRoot(FilesPath));

                if (data < bytes.Length)
                {
                    bytes = new byte[data];
                    rand.NextBytes(bytes);
                    CreateFile(FilesPath, i.ToString(), bytes);
                    break;
                }

                if (i == 1 || Dispatcher.Invoke(() => RandomDataСheckBox.IsChecked ?? true))
                {
                    rand.NextBytes(bytes);
                }

                CreateFile(FilesPath, i.ToString(), bytes);

                Dispatcher.Invoke(() => ETCTextBlock.Text = "Estimate time: " + timer.GetEtc(i).ToString(@"hh\:mm\:ss"));
            }

            if (Dispatcher.Invoke(() => DeleteTempCheckBox.IsChecked ?? false))
            {
                Dispatcher.Invoke(() => StatusTextBlock.Text = "Deleting temp folder...");

                DeleteDirectory(FilesPath);
            }

            IsFormatting = false;

            Dispatcher.Invoke(() => Status.Value = 100);
            Dispatcher.Invoke(() => StatusTextBlock.Text = "Completed!");
            Dispatcher.Invoke(() => ETCTextBlock.Text = String.Empty);
        }

        #region Clicks

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            fd.FileName = "Select Folder";
            fd.InitialDirectory = PathTextBlock.Text;

            if (fd.ShowDialog() == true)
            {
                PathTextBlock.Text = Path.GetDirectoryName(fd.FileName);

                FilesPath = PathTextBlock.Text + @"\Temp\";
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFolderButton.IsEnabled = false;
            StartButton.IsEnabled = false;
            UseCurrentFolderButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            RandomDataСheckBox.IsEnabled = false;
            FileSizeTextBox.IsEnabled = false;

            await Task.Run(() => StartFormat());

            SelectFolderButton.IsEnabled = true;
            StartButton.IsEnabled = true;
            UseCurrentFolderButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            RandomDataСheckBox.IsEnabled = true;
            FileSizeTextBox.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IsFormatting = false;
        }

        private void UseCurrentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            PathTextBlock.Text = System.AppDomain.CurrentDomain.BaseDirectory;

            FilesPath = PathTextBlock.Text + @"\Temp\";
        }

        #endregion Clicks
    }
}