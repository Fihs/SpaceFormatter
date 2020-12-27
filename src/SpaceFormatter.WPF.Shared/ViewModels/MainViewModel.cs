using Microsoft.Win32;
using SpaceFormatter.Core;
using SpaceFormatter.WPF.Shared.Commands;
using System;
using System.ComponentModel;
using System.Threading;

namespace SpaceFormatter.WPF.Shared.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly Formatter formatter;

        private CancellationTokenSource cancellationTokenSource;

        private readonly OpenFileDialog openFileDialog;

        #endregion Fields

        #region Constructors

        public MainViewModel()
        {
            formatter = new Formatter();

            openFileDialog = new OpenFileDialog
            {
                InitialDirectory = System.IO.Path.GetPathRoot(Path),
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            Path = AppDomain.CurrentDomain.BaseDirectory;
            Clear = true;
            Random = false;
            FileSize = 10485760L;

            SelectFolderCommand = new RelayCommand(SelectFolderCanExecute, SelectFolderExecute);
            StartCommand = new RelayCommand(StartCanExecute, StartExecute);
            StopCommand = new RelayCommand(StopCanExecute, StopExecute);
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

        #region Commands

        public RelayCommand SelectFolderCommand { get; }
        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }

        #endregion Commands

        #region Path

        private string path;

        public string Path
        {
            get => path;
            set
            {
                if (path == value)
                {
                    return;
                }

                path = value;
                OnPropertyChanged();
            }
        }

        #endregion Path

        #region Clear

        private bool clear;

        public bool Clear
        {
            get => clear;
            set
            {
                if (clear == value)
                {
                    return;
                }

                clear = value;
                OnPropertyChanged();
            }
        }

        #endregion Clear

        #region Random

        private bool random;

        public bool Random
        {
            get => random;
            set
            {
                if (random == value)
                {
                    return;
                }

                random = value;
                OnPropertyChanged();
            }
        }

        #endregion Random

        #region FileSize

        private ulong fileSize;

        public ulong FileSize
        {
            get => fileSize;
            set
            {
                if (fileSize == value)
                {
                    return;
                }

                fileSize = value;

                OnPropertyChanged();
            }
        }

        #endregion FileSize

        #region Progress

        private FormatterProgress progress;

        public FormatterProgress Progress
        {
            get => progress;
            set
            {
                if (progress == value)
                {
                    return;
                }

                progress = value;
                OnPropertyChanged();
            }
        }

        #endregion Progress

        #region IsFormatting

        private bool isFormatting;

        public bool IsFormatting
        {
            get => isFormatting;
            set
            {
                if (isFormatting == value)
                {
                    return;
                }

                isFormatting = value;
                OnPropertyChanged();
            }
        }

        #endregion IsFormatting

        #endregion Properties

        #region Methods

        #region SelectFolderCommand

        private void SelectFolderExecute(object parameter)
        {
            var path = parameter as string;

            if (path != null)
            {
                openFileDialog.FileName = "*";
                openFileDialog.InitialDirectory = path;

                if (openFileDialog.ShowDialog() == true)
                {
                    Path = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                }
            }
            else
            {
                Path = System.IO.Path.GetPathRoot(Path);
            }
        }

        private bool SelectFolderCanExecute(object parameter)
        {
            if (!IsFormatting)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion SelectFolderCommand

        #region StartCommand

        private async void StartExecute(object parameter)
        {
            FormatterParameters parameters = new FormatterParameters(Path, Clear, Random, FileSize);
            IProgress<FormatterProgress> progress = new Progress<FormatterProgress>((e) => Progress = e);
            cancellationTokenSource = new CancellationTokenSource();
            IsFormatting = true;
            await formatter.Format(parameters, progress, cancellationTokenSource.Token).ConfigureAwait(false);
            IsFormatting = false;
        }

        private bool StartCanExecute(object parameter)
        {
            if (!IsFormatting)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion StartCommand

        #region StopCommand

        private void StopExecute(object parameter)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
            IsFormatting = false;
        }

        private bool StopCanExecute(object parameter)
        {
            if (IsFormatting && cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion StopCommand

        #endregion Methods
    }
}