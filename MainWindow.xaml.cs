using Dbf2XlsxConverter.Models;
using Dbf2XlsxConverter.ViewModels;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;

namespace Dbf2XlsxConverter
{
    public partial class MainWindow : Window
    {
        MainViewModel ViewModel => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(ViewModel.OutputFolder))
                ViewModel.OutputFolder = Directory.GetCurrentDirectory();
        }

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "DBF файлы (*.dbf)|*.dbf|Все файлы (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                AddFiles(dialog.FileNames);
        }

        private void SelectOutput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = ViewModel.OutputFolder;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ViewModel.OutputFolder = dialog.SelectedPath;
        }

        private void Files_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Files_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(x => x.EndsWith(".dbf", System.StringComparison.OrdinalIgnoreCase)).ToArray();
                AddFiles(files);
            }
        }

        private void AddFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (!ViewModel.FilesQueue.Any(f => f.SourcePath == file))
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
