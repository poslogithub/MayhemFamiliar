using System.Text;

namespace MayhemFamiliar
{
    public partial class MainForm : Form
    {
        // private CardData? _cardData;
        private LogWatcher? _logWatcer;
        private JsonParser? _jsonParser;
        private DialogueGenerator? _dialogueGenerator;
        private Speaker? _speaker;
        private CancellationTokenSource? _ctsLogWatcher, _ctsJsonParser, _ctsDialogueGenerator, _ctsSpeaker;
        private const string DetailedLogEnable = "DETAILED LOGS: ENABLED";
        private const string DefaultLogFileName = "Player.log";
        private static readonly string DefaultUserLogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "Wizards Of The Coast", "MTGA"
        );
        private string _logFilePath;

        public MainForm()
        {
            InitializeComponent();
            this.Shown += Form_Shown;
            this.FormClosing += Form_FormClosing;
        }
        private void Form_Shown(object sender, EventArgs e)
        {
            // var configJson = JObject.Parse(@"{""Config"": { ""MtgaDataDirPath"": ""C:\\Program Files\\Wizards of the Coast\\MTGA\\MTGA_Data\\Downloads\\Raw\\Raw_CardDatabase_96e0ea3603a307b5cd9a2de7f4dbcafc.mtga"", ""MtgaLogDirPath"": ""C:\\Program Files\\Wizards of the Coast\\MTGA\\MTGA_Data\\Logs\\Logs"" } }");

            // SQLiteデータファイルのパスを指定してください
            // string dbFilePath = @"C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw\Raw_CardDatabase_96e0ea3603a307b5cd9a2de7f4dbcafc.mtga";
            // _cardData = new CardData(dbFilePath);
            Logger.Initialize(LogToTextBox);

            try
            {
                _logFilePath = GetLogFilePath();
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ログディレクトリ {ex.Message} が見つかりません", LogLevel.Error);
                MessageBox.Show($"ログディレクトリ {ex.Message} が見つかりません。アプリケーションを終了します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            catch (FileNotFoundException ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ログファイル {ex.Message} が見つかりません: {ex.Message}", LogLevel.Error);
                MessageBox.Show("ログファイルが見つかりません。アプリケーションを終了します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            Logger.Instance.Log($"{this.GetType().Name}: ログファイル: {_logFilePath}");

            // 詳細ログ有効確認
            if (CheckDetailedLog())
            {
                Logger.Instance.Log($"{this.GetType().Name}: 詳細ログが有効");
            }
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: 詳細ログが無効");
            }


            _logWatcer = new LogWatcher(_logFilePath);
            _ctsLogWatcher = new CancellationTokenSource();
            Task.Run(() => _logWatcer.Start(_ctsLogWatcher.Token));

            _jsonParser = new JsonParser();
            _ctsJsonParser = new CancellationTokenSource();
            Task.Run(() => _jsonParser.Start(_ctsJsonParser.Token));

            _dialogueGenerator = new DialogueGenerator();
            _ctsDialogueGenerator = new CancellationTokenSource();
            Task.Run(() => _dialogueGenerator.Start(_ctsDialogueGenerator.Token));

            _speaker = new Speaker();
            _ctsSpeaker = new CancellationTokenSource();
            Task.Run(() => _speaker.Start(_ctsSpeaker.Token));
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ctsLogWatcher?.Cancel();
            _ctsLogWatcher?.Dispose();
        }

        // TextBoxにログを追加（UIスレッドで安全に実行）
        private void LogToTextBox(string message)
        {
            AppendLog(message);
        }

        private void AppendLog(string message)
        {
            if (textBoxLog.InvokeRequired)
            {
                textBoxLog.Invoke(() =>
                {
                    textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                    textBoxLog.SelectionStart = textBoxLog.TextLength;
                    textBoxLog.ScrollToCaret();
                });

            }
            else
            {
                textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
        }

        private string GetLogFilePath()
        {
            string logDirectory;
            // ログディレクトリ確定
            if (Directory.Exists(DefaultUserLogDirectory))
            {
                logDirectory = DefaultUserLogDirectory;
            }
            else
            {
                logDirectory = SelectDirectory();
            }
            if (string.IsNullOrEmpty(logDirectory))
            {
                throw new DirectoryNotFoundException(logDirectory);
            }

            // ログファイル確定
            /*
            logFile = this.GetLatestLogFile(logDirectory);
            if (String.IsNullOrEmpty(_logFilePath))
            {
                throw new FileNotFoundException(logDirectory);
            }
            */
            return Path.Combine(logDirectory, DefaultLogFileName);
        }

        private string GetLatestLogFile(string logDirectory)
        {
            if (!Directory.Exists(logDirectory))
            {
                return "";
            }

            return Directory
                .GetFiles(logDirectory, "*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault()?.FullName ?? "";
        }

        private static string SelectDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                // ダイアログの設定（オプション）
                dialog.ShowNewFolderButton = false; // 新しいフォルダ作成ボタンの表示（デフォルト: true）

                // ダイアログを表示
                DialogResult result = dialog.ShowDialog();

                // ユーザーが「OK」をクリックした場合、選択されたパスを返す
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }

                // キャンセルまたは有効なパスが選択されなかった場合
                return "";
            }
        }
        private Boolean CheckDetailedLog()
        {
            try
            {
                // ファイルサイズを取得
                var fileInfo = new FileInfo(_logFilePath);
                // 追記部分を読み込む
                using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string? line = reader.ReadLine();
                    while (line is not null)
                    {
                        if (line.Contains(DetailedLogEnable))
                        {
                            return true;
                        }
                        line = reader.ReadLine();
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"{this.GetType().Name}: 詳細ログ有効確認で例外が発生");
                Console.WriteLine($"{this.GetType().Name}: 読み込みエラー: {ex.Message}");
            }
            return false;
        }

    }
}
