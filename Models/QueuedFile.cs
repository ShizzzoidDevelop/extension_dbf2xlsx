using System;

namespace Dbf2XlsxConverter.Models
{
    public enum FileStatus
    {
        Queued,
        Processing,
        Completed,
        Error
    }

    public class QueuedFile
    {
        public string SourcePath { get; set; }      // Полный путь к исходному dbf
        public string TargetPath { get; set; }      // Полный путь к результирующему xlsx
        public FileStatus Status { get; set; }      // Текущий статус
        public int Progress { get; set; }           // Прогресс (0-100)
        public TimeSpan? Duration { get; set; }     // Длительность конвертации
        public string ErrorMessage { get; set; }    // Сообщение об ошибке
        public Guid TaskId { get; set; } = Guid.NewGuid(); // Для идентификации в очереди
    }
}
