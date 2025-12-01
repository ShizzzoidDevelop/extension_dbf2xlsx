using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dbf2XlsxConverter.Models;
using Net.SourceForge.Koogra.DBF; // DotNetDBF
using ClosedXML.Excel;

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
            {
                Parallel.ForEach(queue, options, file =>
                {
                    if (file.Status != FileStatus.Queued) return;
                    file.Status = FileStatus.Processing;
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        string outPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(file.SourcePath) + ".xlsx");
                        file.TargetPath = outPath;
                        using var fs = File.OpenRead(file.SourcePath);
                        // --- Определение кодировки по заголовку или Heuristic:
                        System.Text.Encoding encoding = GuessDbfEncoding(file.SourcePath);
                        using var dbfReader = new DotNetDBF.DBFReader(fs)
                        {
                            // FoxPro заметить: MemoSupport=ON автоматически
                            Charset = encoding
                        };
                        // --- ClosedXML workbook:
                        using var wb = new XLWorkbook();
                        var ws = wb.Worksheets.Add("Лист1");
                        // --- Заголовки
                        for (int col = 0; col < dbfReader.FieldCount; col++)
                        {
                            ws.Cell(1, col + 1).Value = dbfReader.Fields[col].Name;
                        }
                        // --- Данные
                        int row = 2;
                        object[] record;
                        while ((record = dbfReader.NextRecord()) != null)
                        {
                            for (int col = 0; col < record.Length; col++)
                                ws.Cell(row, col + 1).Value = record[col];
                            file.Progress = Math.Min(99, file.Progress + 1); // индикатор (будет обнулён ниже)
                            row++;
                        }
                        // --- Красиво автоширим:
                        ws.Columns().AdjustToContents();
                        wb.SaveAs(outPath);
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
                });
            });
        }

        /// <summary>
        /// Простейший автоопределитель кодировки DBF (win/oem)
        /// </summary>
        private static System.Text.Encoding GuessDbfEncoding(string dbfPath)
        {
            try
            {
                using var fs = File.OpenRead(dbfPath);
                if (fs.Length < 30) return System.Text.Encoding.Default;
                fs.Seek(29, SeekOrigin.Begin);
                int marker = fs.ReadByte();
                // 0x03, 0x30 обычно WIN1251, 0x26 — OEM866
                if (marker == 0x03 || marker == 0x30)
                    return System.Text.Encoding.GetEncoding("windows-1251");
                if (marker == 0x26)
                    return System.Text.Encoding.GetEncoding(866);
                // fallback (чаще всего WIN1251):
                return System.Text.Encoding.GetEncoding("windows-1251");
            }
            catch
            {
                return System.Text.Encoding.Default;
            }
        }
    }
}
