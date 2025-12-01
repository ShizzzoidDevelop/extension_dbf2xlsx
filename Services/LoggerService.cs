using System;
using Serilog;

namespace Dbf2XlsxConverter.Services
{
    public class LoggerService
    {
        private static LoggerService _instance;
        private static readonly object _lock = new();
        private readonly ILogger _logger;

        private LoggerService()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static LoggerService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new LoggerService();
                }
            }
        }

        public void LogInfo(string message) => _logger.Information(message);
        public void LogError(string message, Exception ex = null) => _logger.Error(ex, message);
        public void LogOperationResult(string src, string dst, TimeSpan dur, Exception err = null)
        {
            if (err == null)
                LogInfo($"УСПЕХ: {src} -> {dst}, {dur:hh\\:mm\\:ss}");
            else
                LogError($"ОШИБКА: {src} -> {dst}, {dur:hh\\:mm\\:ss}: {err.Message}", err);
        }
    }
}
