using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Dbf2XlsxConverter.ViewModels;
using Dbf2XlsxConverter.Models;
using System;
using System.Threading.Tasks;

namespace Dbf2XlsxConverter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel => DataContext as MainViewModel;

        private async void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilesPickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = "Выберите .dbf-файлы",
                FileTypeFilter = new[] { FilePickerFileTypes.All, new FilePickerFileType("DBF files") { Patterns = new[] { "*.dbf" } } }
            });
            AddFiles(files.Select(f => f.Path.LocalPath).ToArray());
        }

        private async void SelectOutput_Click(object sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Папка для сохранения .xlsx"
            });
            if (folders?.FirstOrDefault() is { } folder)
            {
                ViewModel.OutputFolder = folder.Path.LocalPath;
            }
        }

        private void Files_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
                e.DragEffects = DragDropEffects.Copy;
            else
                e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }

        private async void Files_Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var files = (await e.Data.GetFileNamesAsync())?.Where(f => f.EndsWith(".dbf", StringComparison.OrdinalIgnoreCase))?.ToArray();
                AddFiles(files);
            }
        }

        private void AddFiles(IEnumerable<string>? files)
        {
            if (files == null) return;
            foreach (var file in files)
            {
                if (!ViewModel.FilesQueue.Any(q => q.SourcePath == file))
                {
                    ViewModel.FilesQueue.Add(new QueuedFile
                    {
                        SourcePath = file,
                        Status = FileStatus.Queued,
                        Progress = 0
                    });
                }
            }
        }
    }
}
