using System;
using System.IO;
using System.Threading.Tasks;

namespace JOJO.Mes.Log
{
    internal static class MesLog
    {
        public enum LogLevel
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
        private static string logBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "Meslog");
        private static long maxFileSize = 5 * 1024 * 1024; // 5MB
        private static LogLevel minimumLevel = LogLevel.Trace;

        static MesLog()
        {
            if (!Directory.Exists(logBasePath))
            {
                Directory.CreateDirectory(logBasePath);
            }
        }

        private static string GetLogFilePath()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(logBasePath, $"{date}.txt");
        }

        private static async Task AppendTextAsync(string text, string filePath)
        {
            var fileOptions = FileOptions.Asynchronous;
            using (var fileStream = new FileStream(
                filePath,
                FileMode.Append, // 使用FileMode.Append以追加模式打开文件
                FileAccess.Write,
                FileShare.ReadWrite,
                bufferSize: 4096 * 10,
                fileOptions))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    // 异步写入文本到文件
                    await streamWriter.WriteAsync(text);
                }
            }
        }

        public static async void Write(LogLevel level, string message)
        {
            if (level < minimumLevel)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyy - MM - dd HH:mm:ss");
            string logMessage = $"{timestamp} [{level}]: {message}{Environment.NewLine}";
            string filePath = GetLogFilePath();

            if (File.Exists(filePath) && new FileInfo(filePath).Length >= maxFileSize)
            {
                filePath = Path.Combine(logBasePath, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");
            }

            await AppendTextAsync(logMessage, filePath);
        }

        public static void Trace(string message)
        {
            Write(LogLevel.Trace, message);
        }

        public static void Debug(string message)
        {
#if DEBUG
            Write(LogLevel.Debug, message);
#endif
        }

        public static void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        public static void Warn(string message)
        {
            Write(LogLevel.Warn, message);
        }

        public static void Error(string message)
        {
            Write(LogLevel.Error, message);
        }

        public static void Fatal(string message)
        {
            Write(LogLevel.Fatal, message);
        }

    }
}
