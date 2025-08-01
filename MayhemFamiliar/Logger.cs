using System;
using System.IO;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    internal static class LogLevel
    {
        public const string Info = "INFO";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Debug = "DEBUG";
    }
    internal class Logger : IDisposable
    {
        private static readonly string LogFileName = $"{Application.ProductName}.log";
        private static readonly object _lock = new object();
        private static Logger _instance;
        private readonly Action<string> _log;
        private static StreamWriter _writer;
        private bool _disposed = false;

        // プライベートコンストラクタ
        private Logger(Action<string> logAction)
        {
            _log = logAction;
        }

        // インスタンス取得用プロパティ
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Logger is not initialized. Call Initialize() first.");
                }
                return _instance;
            }
        }

        // 初期化メソッド（引数を渡す）
        public static Boolean Initialize(Action<string> _log)
        {
            lock (_lock) // スレッドセーフ
            {
                try
                {
                    if (_instance != null)
                    {
                        throw new InvalidOperationException("Logger is already initialized.");
                    }
                    _instance = new Logger(_log);

                    // ログファイルの初期化
                    _writer = new StreamWriter(LogFileName, append: false) { AutoFlush = true };
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public void Log(string message, string level = LogLevel.Info)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            if (level != LogLevel.Debug)
            {
                _log?.Invoke(logMessage);
            }
            _writer?.WriteLine(logMessage);

        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _writer?.Close();
                    _writer?.Dispose();
                }
                finally
                {
                    _writer = null;
                    _disposed = true;
                }
            }
        }
    }
}
