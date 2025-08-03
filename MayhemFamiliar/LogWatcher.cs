using MayhemFamiliar.QueueManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MayhemFamiliar
{
    internal class LogWatcher : IDisposable
    {
        private const string PowerShellExecutable = "powershell.exe";
        private readonly Process _powershell = new Process();
        private readonly string _logFilePath;

        public LogWatcher(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Dispose()
        {
            ConsoleHelper.SendCtrlC(_powershell);
        }

        public async void Start(CancellationToken _ctsLogWatcher, Boolean readFullLog = false)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 開始");

            _powershell.StartInfo.FileName = PowerShellExecutable;
            _powershell.StartInfo.Arguments = $"-NoProfile -Command \"Get-Content -Path '{_logFilePath}' -Tail 1 -Wait\"";
            if (readFullLog)
            {
                _powershell.StartInfo.Arguments = $"-NoProfile -Command \"Get-Content -Path '{_logFilePath}'\"";
            }
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
                StreamReader reader = _powershell.StandardOutput;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // _log.Invoke($"{this.GetType().Name}: {line}");
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

    public static class ConsoleHelper
    {
        [DllImport("kernel32.dll")]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        private const uint CTRL_C_EVENT = 0;

        public static void SendCtrlC(Process process)
        {
            // 既存のコンソールを切り離し
            FreeConsole();
            // 対象プロセスのコンソールにアタッチ
            if (AttachConsole((uint)process.Id))
            {
                // プロセスグループID=0で全体に送信
                GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
                // 必要なら少し待つ
                System.Threading.Thread.Sleep(100);
                // 切り離し
                FreeConsole();
            }
        }
    }
}