using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Dbf2XlsxConverter.Models;
using Dbf2XlsxConverter.Services;
using System.Linq;

namespace Dbf2XlsxConverter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<QueuedFile> FilesQueue { get; set; } = new();
        public ICommand AddFilesCommand { get; }
        public ICommand StartConversionCommand { get; }
        public ICommand RemoveFileCommand { get; }

        int _maxDegreeOfParallelism = Environment.ProcessorCount;
        public int MaxDegreeOfParallelism
        {
            get => _maxDegreeOfParallelism;
            set { _maxDegreeOfParallelism = value; OnPropertyChanged(); }
        }

        string _outputFolder = System.IO.Directory.GetCurrentDirectory();
        public string OutputFolder
        {
            get => _outputFolder;
            set { _outputFolder = value; OnPropertyChanged(); }
        }

        bool _isConverting = false;
        public bool IsConverting
        {
            get => _isConverting;
            set { _isConverting = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            AddFilesCommand = new RelayCommand(AddFiles);
            StartConversionCommand = new RelayCommand(async _ => await StartConversion(), _ => FilesQueue.Count > 0 && !IsConverting);
            RemoveFileCommand = new RelayCommand(RemoveFile, _ => !IsConverting);
        }

        private void AddFiles(object param)
        {
            // Заполняется через поля и методы View, здесь только добавление
            if (param is string[] files)
            {
                foreach (var f in files)
                {
                    if (!FilesQueue.Any(q => q.SourcePath == f))
                        FilesQueue.Add(new QueuedFile { SourcePath = f, Status = FileStatus.Queued, Progress = 0 });
                }
            }
        }

        private void RemoveFile(object param)
        {
            if (param is QueuedFile file && FilesQueue.Contains(file))
                FilesQueue.Remove(file);
        }

        private async Task StartConversion()
        {
            IsConverting = true;
            var converter = new DbfToXlsxConverter(MaxDegreeOfParallelism, OutputFolder);
            await converter.ConvertQueueAsync(FilesQueue);
            IsConverting = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
