using MayhemFamiliar.QueueManager;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class LogWatcher
    {
        private readonly string _logDirectory;
        private string _DefaultAppLogDirectory = @"C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Logs\Logs";
        private string _DefaultUserLogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"LocalLow\Wizards Of The Coast\MTGA"
        );
        private readonly Action<string> _log;
        private FileSystemWatcher? _watcher;
        private static long _lastFileSize = 0; // 最後に読み取ったファイルサイズ

        // [0] DETAILED LOGS: DISABLED

        public LogWatcher(Action<string> logAction, string logDirectory = "")
        {
            _log = logAction ?? throw new ArgumentNullException(nameof(logAction));

            if (String.IsNullOrEmpty(logDirectory))
            {
                // TODO ログディレクトリの存在チェックから入るよ
                if (Directory.Exists(_DefaultAppLogDirectory))
                {
                    _logDirectory = _DefaultAppLogDirectory;
                }
                else if (Directory.Exists(_DefaultUserLogDirectory))
                {
                    _logDirectory = _DefaultUserLogDirectory;
                }
                else
                {
                    // TODO ログディレクトリが見つからない場合の処理...本来はMainFormで設定するべき
                    throw new DirectoryNotFoundException("ログディレクトリが見つかりません。");
                }
            }
            else
            {
                _logDirectory = logDirectory;
            }
        }

        public void Start()
        {
            _log?.Invoke("LogWatcher: 開始");
            // 最新の.logファイルを取得
            var filePath = GetLatestLogFile();
            if (filePath == null)
            {
                _log?.Invoke("LogWatcher: .logファイルが見つかりません");
                return;
            }
            // _lastFileSize = new FileInfo(filePath).Length;   最初から読み込む。

            _log?.Invoke($"LogWatcher: {filePath} の監視を開始");
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += OnChanged;
            _watcher.EnableRaisingEvents = true;

            // 初期読み込み
            var args = new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            OnChanged(_watcher, args);
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // ファイルサイズを取得
                var fileInfo = new FileInfo(e.FullPath);
                long currentFileSize = fileInfo.Length;

                // ファイルサイズが増加した場合（追記があった場合）
                if (currentFileSize > _lastFileSize)
                {
                    // 追記部分を読み込む
                    using (var fileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string? jsonBuilder = null;

                        // ファイルポインタを前回のサイズ位置に移動
                        fileStream.Seek(_lastFileSize, SeekOrigin.Begin);
                        // 追記部分を読み込む
                        string? line = reader.ReadLine();
                        while (line is not null)
                        {
                            if (line.StartsWith("{") && line.EndsWith("}"))
                            {
                                // 単一行JSON
                                JsonQueue.Queue.Enqueue(line);
                            }
                            else if (line.StartsWith("{"))
                            {
                                // 複数行JSONの開始
                                jsonBuilder = line;
                            }
                            else if (jsonBuilder != null)
                            {
                                // 複数行JSONの途中
                                jsonBuilder += line;
                                if (line == "}")
                                {
                                    // 複数行JSONの終了
                                    JsonQueue.Queue.Enqueue(jsonBuilder);
                                    jsonBuilder = null;
                                }
                            }
                            // それ以外の行は無視
                            line = reader.ReadLine();
                        }
                    }
                    // 現在のファイルサイズを保存
                    _lastFileSize = currentFileSize;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"読み込みエラー: {ex.Message}");
            }
        }
        private string? GetLatestLogFile()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    _log?.Invoke($"LogTailer: ディレクトリ {_logDirectory} が見つかりません");
                    return null;
                }

                return Directory
                    .GetFiles(_logDirectory, "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault()?.FullName;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"LogTailer: 最新ファイル取得エラー: {ex.Message}");
                return null;
            }
        }
    }
}