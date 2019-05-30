using System;
using System.ComponentModel;
using System.IO;

namespace SpaceFormatter
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Constructors

        public MainViewModel()
        {
            FilesPath = AppDomain.CurrentDomain.BaseDirectory;
            FileSize = "10485760";
            DeleteTemp = true;

            rand = new Random();

            fd = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Folder",
                InitialDirectory = Path.GetPathRoot(FilesPath),
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Events

        #region Properties

        private readonly Microsoft.Win32.OpenFileDialog fd;
        private readonly Random rand;

        private string tempFilesPath;

        #region FilesPath

        private string filesPath;

        public string FilesPath
        {
            get => filesPath;
            set
            {
                if (filesPath == value)
                    return;

                filesPath = value;
                tempFilesPath = value + @"\Temp\";
                OnPropertyChanged();
            }
        }

        #endregion FilesPath

        #region IsFormatting

        private bool isFormatting;

        public bool IsFormatting
        {
            get => isFormatting;
            set
            {
                if (isFormatting == value)
                    return;

                isFormatting = value;
                OnPropertyChanged();
            }
        }

        #endregion IsFormatting

        #region FileSize

        private ulong fileSize;

        public string FileSize
        {
            get => fileSize.ToString();
            set
            {
                if (!ulong.TryParse(value, out ulong temp))
                {
                    fileSize = 10485760;
                }
                else
                {
                    if (fileSize == temp)
                        return;

                    fileSize = temp;
                }

                OnPropertyChanged();
            }
        }

        #endregion FileSize

        #region StatusText

        private string statusText;

        public string StatusText
        {
            get => statusText;
            set
            {
                if (statusText == value)
                    return;

                statusText = value;
                OnPropertyChanged();
            }
        }

        #endregion StatusText

        #region StatusProgress

        private double statusProgress;

        public double StatusProgress
        {
            get => statusProgress;
            set
            {
                if (statusProgress == value)
                    return;

                statusProgress = value;
                OnPropertyChanged();
            }
        }

        #endregion StatusProgress

        #region RandomData

        private bool randomData;

        public bool RandomData
        {
            get => randomData;
            set
            {
                if (randomData == value)
                    return;

                randomData = value;
                OnPropertyChanged();
            }
        }

        #endregion RandomData

        #region DeleteTemp

        private bool deleteTemp;

        public bool DeleteTemp
        {
            get => deleteTemp;
            set
            {
                if (deleteTemp == value)
                    return;

                deleteTemp = value;
                OnPropertyChanged();
            }
        }

        #endregion DeleteTemp

        #region EstimateTime

        private string estimateTime;

        public string EstimateTime
        {
            get => estimateTime;
            set
            {
                if (estimateTime == value)
                    return;

                estimateTime = value;
                OnPropertyChanged();
            }
        }

        #endregion EstimateTime

        #endregion Properties

        #region Methods

        public void SelectFolder(string path)
        {
            fd.FileName = "Select Folder";
            fd.InitialDirectory = path;

            if (fd.ShowDialog() == true)
            {
                FilesPath = Path.GetDirectoryName(fd.FileName);
            }
        }

        public void StartFormatting(System.Threading.CancellationToken cancelToken)
        {
            System.Threading.Tasks.Task.Run(() => Format(cancelToken));
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
        }

        private void CreateFile(string path, string name, byte[] bytes)
        {
            try
            {
                // Determine whether the directory exists.
                if (File.Exists(path + name))
                {
                    Console.WriteLine("That file exists already.");
                    name = Path.GetRandomFileName();
                }

                File.WriteAllBytes(path + name, bytes);
                Console.WriteLine("The file {0} was created successfully.", name);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
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
        }

        private void Format(System.Threading.CancellationToken cancelToken)
        {
            IsFormatting = true;

            StatusProgress = 0;

            var data = GetDriveFreeSpace(Path.GetPathRoot(tempFilesPath));

            if (data == -1 || data == 0)
            {
                StatusText = "Error";
                return;
            }

            StatusText = "Creating temp folder...";

            CreateDirectory(tempFilesPath);

            var count = (data / (long)fileSize) + 1;

            var timer = new EtcCalculator((int)count);

            byte[] bytes = new byte[fileSize];

            for (int i = 1; i <= count; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                StatusProgress = i / ((double)count / 100);
                StatusText = $"Creating temp files... {i}\\{count}";

                data = GetDriveFreeSpace(Path.GetPathRoot(tempFilesPath));

                if (data < bytes.Length)
                {
                    bytes = new byte[data];
                    rand.NextBytes(bytes);
                    CreateFile(tempFilesPath, i.ToString(), bytes);
                    break;
                }

                if (i == 1 || RandomData)
                {
                    rand.NextBytes(bytes);
                }

                CreateFile(tempFilesPath, i.ToString(), bytes);

                EstimateTime = "Estimate time: " + timer.GetEtc(i).ToString(@"hh\:mm\:ss");
            }

            if (DeleteTemp)
            {
                StatusText = "Deleting temp folder...";

                DeleteDirectory(tempFilesPath);
            }

            IsFormatting = false;

            StatusProgress = 100;
            StatusText = "Completed!";
            EstimateTime = String.Empty;
        }

        private long GetDriveFreeSpace(string driveName)
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.Name == driveName)
                    {
                        return drive.AvailableFreeSpace;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            return -1;
        }

        #endregion Methods
    }
}