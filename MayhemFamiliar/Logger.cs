using System;

namespace MayhemFamiliar
{
    internal static class LogLevel
    {
        public const string Info = "INFO";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Debug = "DEBUG";
    }
    internal class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();
        private readonly Action<string> _log;

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
        public static void Initialize(Action<string> _log)
        {
            lock (_lock) // スレッドセーフ
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("Logger is already initialized.");
                }
                _instance = new Logger(_log);
            }
        }
        public void Log(string message, string level = LogLevel.Info)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            _log?.Invoke(logMessage);
        }
    }
}
