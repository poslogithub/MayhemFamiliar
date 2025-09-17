using MayhemFamiliar.Logging;
using MayhemFamiliar.Queues;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace MayhemFamiliar
{
    internal class LogWatcher : IDisposable
    {
        private const string PowerShellExecutable = "powershell.exe";
        private const string DraftPackStartsWith = "[UnityCrossThreadLogger]Draft.Notify ";
        private const string DraftPackStartPattern = @"\[UnityCrossThreadLogger\]Draft\.Notify ";
        private const string DraftPickStartsWith = "[UnityCrossThreadLogger]==> EventPlayerDraftMakePick ";
        private const string DraftPickStartPattern = @"\[UnityCrossThreadLogger\]==> EventPlayerDraftMakePick ";
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
                    // ドラフト
                    if (line.StartsWith(DraftPackStartsWith))
                    {
                        jsonBuilder = Regex.Replace(line, DraftPackStartPattern, "");   // ログの先頭部分（JSONでない箇所）を削除
                        Enqueue(jsonBuilder);
                        continue;
                    }
                    else if (line.StartsWith(DraftPickStartsWith))
                    {
                        jsonBuilder = Regex.Replace(line, DraftPickStartPattern, "");   // ログの先頭部分（JSONでない箇所）を削除
                        Enqueue(jsonBuilder);
                        continue;
                    }

                    // ゲームプレイ
                    if (line.StartsWith("{") && line.EndsWith("}"))
                    {
                        // 単一行JSON
                        Enqueue(line);
                        continue;
                    }
                    else if (line.StartsWith("{"))
                    {
                        // 複数行JSONの開始
                        jsonBuilder = line;
                        continue;
                    }
                    else if (jsonBuilder != null)
                    {
                        // 複数行JSONの途中
                        jsonBuilder += line;
                        if (line == "}")
                        {
                            // 複数行JSONの終了
                            Enqueue(jsonBuilder);
                            jsonBuilder = "";
                        }
                        continue;
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
        private Boolean Enqueue(string jsonString)
        {
            try
            {
                JObject json = JObject.Parse(jsonString);
                JsonQueue.Queue.Enqueue(json);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: JSONのパースに失敗: {jsonString}");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
                return false;
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
                Thread.Sleep(100);
                // 切り離し
                FreeConsole();
            }
        }
    }
}