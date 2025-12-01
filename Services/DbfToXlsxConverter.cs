using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Dbf2XlsxConverter.Models;

namespace Dbf2XlsxConverter.Services
{
    public class DbfToXlsxConverter
    {
        private readonly int _parallelism;
        private readonly string _outputDir;

        public DbfToXlsxConverter(int parallelism, string outputDir)
        {
            _parallelism = parallelism;
            _outputDir = outputDir;
        }

        public async Task ConvertQueueAsync(ObservableCollection<QueuedFile> queue)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = _parallelism };
            await Task.Run(() =>
                Parallel.ForEach(queue, options, file =>
                {
                    if (file.Status != FileStatus.Queued)
                        return;
                    file.Status = FileStatus.Processing;
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        string outPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(file.SourcePath) + ".xlsx");
                        file.TargetPath = outPath;
                        // TODO: Реализация преобразования DBF → XLSX через DotNetDBF + ClosedXML
                        // (После подключения пакетов заполним подробно)

                        file.Progress = 100;
                        file.Status = FileStatus.Completed;
                        file.Duration = sw.Elapsed;
                        LoggerService.Instance.LogOperationResult(file.SourcePath, file.TargetPath, sw.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        file.Status = FileStatus.Error;
                        file.ErrorMessage = ex.Message;
                        file.Progress = 0;
                        file.Duration = sw.Elapsed;
                        LoggerService.Instance.LogOperationResult(file.SourcePath, file.TargetPath, sw.Elapsed, ex);
                    }
                })
            );
        }
    }
}
