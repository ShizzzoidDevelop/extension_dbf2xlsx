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
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public FileStatus Status { get; set; } = FileStatus.Queued;
        public int Progress { get; set; }
        public TimeSpan? Duration { get; set; }
        public string ErrorMessage { get; set; }
        public Guid TaskId { get; set; } = Guid.NewGuid();
    }
}
