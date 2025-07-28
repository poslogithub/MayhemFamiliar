using MayhemFamiliar.QueueManager;
using System.Diagnostics;

namespace MayhemFamiliar
{
    internal class LogWatcher
    {
        private const string PowerShellExecutable = "powershell.exe";
        private readonly Process _powershell = new Process();
        private readonly string _logFilePath;

        public LogWatcher(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public async void Start(CancellationToken _ctsLogWatcher)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 開始");
            // _lastFileSize = new FileInfo(filePath).Length;   最初から読み込む。

            // 初期読み込み
            // var args = new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            // OnChanged(_watcher, args);

            _powershell.StartInfo.FileName = PowerShellExecutable;
            _powershell.StartInfo.Arguments = $"-NoProfile -Command \"Get-Content -Path '{_logFilePath}' -Tail 1 -Wait\"";
            _powershell.StartInfo.UseShellExecute = false; // シェルを介さず直接実行
            _powershell.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
            _powershell.StartInfo.RedirectStandardError = true; // エラー出力もリダイレクト（必要に応じて）
            _powershell.StartInfo.CreateNoWindow = true; // ウィンドウを表示しない

            try
            {
                _powershell.Start();
                Logger.Instance.Log($"{this.GetType().Name}: {_logFilePath} の監視を開始");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {_powershell.StartInfo.Arguments} 実行時に例外が発生");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
            }


            string jsonBuilder = "";
            string line;
            try
            { 
                using StreamReader reader = _powershell.StandardOutput;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // _log.Invoke($"{this.GetType().Name}: {line}");
                    if (line.StartsWith('{') && line.EndsWith('}'))
                    {
                        // 単一行JSON
                        JsonQueue.Queue.Enqueue(line);
                    }
                    else if (line.StartsWith('{'))
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
                            jsonBuilder = "";
                        }
                    }
                    // それ以外の行は無視
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ログ読み込みで例外が発生");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
            }
        }
    }
}