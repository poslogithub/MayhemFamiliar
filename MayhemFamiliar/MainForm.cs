using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    public partial class MainForm : Form
    {
        private LogWatcher _logWatcer;
        private JsonParser _jsonParser;
        private DialogueGenerator _dialogueGenerator;
        private Speaker _speaker;
        private CancellationTokenSource _ctsLogWatcher, _ctsJsonParser, _ctsDialogueGenerator, _ctsSpeaker;

        private const string MtgaProcessName = "MTGA";

        // LogWatcher用
        private const string DetailedLogEnabled = "DETAILED LOGS: ENABLED";
        private string _mtgaLogFilePath;

        // JsonParser用
        private const string CardDatabaseFileNamePattern = @"Raw_CardDatabase_.*\.mtga";
        private string _cardDatabaseFilePath;


        public MainForm()
        {
            InitializeComponent();
            this.Shown += Form_Shown;
            this.FormClosing += Form_FormClosing;
        }
        private void Form_Shown(object sender, EventArgs e)
        {
            // Logger初期化
            Logger.Initialize(LogToTextBox);

            // TODO: コンフィグ読み込み

            // MTG Arena起動確認
            Logger.Instance.Log($"{this.GetType().Name}: MTG Arena起動確認");
            while (!Util.IsProcessRunning(MtgaProcessName))
            {
                Boolean ignore = false;
                Logger.Instance.Log($"{this.GetType().Name}: MTG Arenaが起動していません。", LogLevel.Error);
                DialogResult result = MessageBox.Show(
                    "MTG Arenaが起動していません。",
                    "MTG Arena起動確認", 
                    MessageBoxButtons.AbortRetryIgnore, 
                    MessageBoxIcon.Warning);
                switch (result)
                {
                    case DialogResult.Retry:
                        Logger.Instance.Log($"{this.GetType().Name}: MTG Arenaの起動を再確認します。");
                        break;
                    case DialogResult.Ignore:
                        Logger.Instance.Log($"{this.GetType().Name}: MTG Arenaの起動を無視します。", LogLevel.Warning);
                        ignore = true;
                        break;
                    default:
                        Logger.Instance.Log($"{this.GetType().Name}: アプリケーションを終了します。");
                        Application.Exit();
                        return;
                }
                if (ignore) break;
            }


            // ログファイル存在確認
            // TODO: 設定ファイルを参照する
            _mtgaLogFilePath = GetInitMtgaLogFilePath();
            while (!File.Exists(_mtgaLogFilePath))
            {
                Logger.Instance.Log($"{this.GetType().Name}: ログファイル {_mtgaLogFilePath} が見つかりません。", LogLevel.Error);
                DialogResult result = MessageBox.Show(
                    $"MTG Arenaのログファイル {_mtgaLogFilePath} が見つかりませんでした。{Environment.NewLine}次に表示されるダイアログで {DefaultValue.MtgaLogFileName} が存在するフォルダを選択してください。", 
                    "MTG Arenaログファイル存在確認", 
                    MessageBoxButtons.OKCancel, 
                    MessageBoxIcon.Warning);
                switch (result)
                {
                    case DialogResult.OK:
                        _mtgaLogFilePath = GetMtgaLogFilePath();
                        break;
                    default:
                        Logger.Instance.Log($"{this.GetType().Name}: アプリケーションを終了します。");
                        Application.Exit();
                        return;
                }
            }
            Logger.Instance.Log($"{this.GetType().Name}: ログファイル: {_mtgaLogFilePath}");

            // 詳細ログ有効確認
            if (CheckDetailedLog())
            {
                Logger.Instance.Log($"{this.GetType().Name}: 詳細ログが有効");
            }
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: 詳細ログが無効");
                MessageBox.Show(
                    $"MTG Arenaの詳細ログが有効化されていません。{Environment.NewLine}MTG Arenaのホーム画面から{Environment.NewLine}[設定] > [アカウント] > [詳細ログ（プラグインのサポート）]{Environment.NewLine}にチェックを入れた後、MTG Arenaと本アプリケーションを再起動してください。", 
                    "MTG Arena 詳細ログ有効確認", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            // カードデータベースファイル存在確認
            // TODO
            _cardDatabaseFilePath = GetInitCardDatabaseFilePath();
            while (!File.Exists(_cardDatabaseFilePath))
            {
                Logger.Instance.Log($"{this.GetType().Name}: カードデータベースファイル {CardDatabaseFileNamePattern} が見つかりません。", LogLevel.Error);
                DialogResult result = MessageBox.Show(
                    $"カードデータベースファイル {CardDatabaseFileNamePattern} が見つかりませんでした。{Environment.NewLine}次に表示されるダイアログで {CardDatabaseFileNamePattern} が存在するフォルダを選択してください。",
                    "カードデータベースファイル存在確認",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);
                switch (result)
                {
                    case DialogResult.OK:
                        _cardDatabaseFilePath = GetCardDatabaseFilePath();
                        break;
                    default:
                        Logger.Instance.Log($"{this.GetType().Name}: アプリケーションを終了します。");
                        Application.Exit();
                        return;
                }
            }
            Logger.Instance.Log($"{this.GetType().Name}: カードデータベースファイル: {_cardDatabaseFilePath}");


            _logWatcer = new LogWatcher(_mtgaLogFilePath);
            _ctsLogWatcher = new CancellationTokenSource();
            Task.Run(() => _logWatcer.Start(_ctsLogWatcher.Token));

            _jsonParser = new JsonParser(_cardDatabaseFilePath);
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
                textBoxLog.Invoke(new Action(() =>
                {
                    textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                    textBoxLog.SelectionStart = textBoxLog.TextLength;
                    textBoxLog.ScrollToCaret();
                }));

            }
            else
            {
                textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
        }

        private string GetInitMtgaLogFilePath()
        {
            // TODO : 設定ファイルを参照する
            return Path.Combine(DefaultValue.MtgaLogDirectory, DefaultValue.MtgaLogFileName);
        }
        private string GetMtgaLogFilePath()
        {
            string logDirectory;

            if (Directory.Exists(DefaultValue.MtgaLogDirectory))
            {
                logDirectory = DefaultValue.MtgaLogDirectory;
            }
            else
            {
                logDirectory = SelectDirectory();
            }
            if (string.IsNullOrEmpty(logDirectory))
            {
                return "";
            }

            return Path.Combine(logDirectory, DefaultValue.MtgaLogFileName);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

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
                var fileInfo = new FileInfo(_mtgaLogFilePath);
                // 追記部分を読み込む
                using (var fileStream = new FileStream(_mtgaLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        if (line.Contains(DetailedLogEnabled))
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

        private string GetInitCardDatabaseFilePath()
        {             // TODO : 設定ファイルを参照する
            return Path.Combine(DefaultValue.CardDatabaseDirectory, GetLargestCardDatabaseFilePath(DefaultValue.CardDatabaseDirectory));
        }
        private string GetLargestCardDatabaseFilePath(string cardDatabaseDirectoryPath)
        {
            // ディレクトリ内のファイルを検索し、正規表現にマッチするもののうち、サイズが最大のものを取得
            // ※稀にサイズが0のファイルが存在するため
            string cardDatabaseFile = "";
            try
            {
                cardDatabaseFile = Directory
                .GetFiles(cardDatabaseDirectoryPath)
                .Where(file => Regex.IsMatch(Path.GetFileName(file), CardDatabaseFileNamePattern))
                .OrderByDescending(file => new FileInfo(file).Length)
                .FirstOrDefault();
            }
            catch
            {
                cardDatabaseFile = "";
            }
            if (String.IsNullOrEmpty(cardDatabaseFile))
            {
                return "";
            }
            return cardDatabaseFile;
        }
        private string GetCardDatabaseFilePath()
        {
            string cardDatabaseDirectoryPath;
            if (Directory.Exists(DefaultValue.CardDatabaseDirectory))
            {
                cardDatabaseDirectoryPath = DefaultValue.CardDatabaseDirectory;
            }
            else
            {
                cardDatabaseDirectoryPath = SelectDirectory();
            }

            if (string.IsNullOrEmpty(cardDatabaseDirectoryPath))
            {
                return "";
            }

            // ディレクトリ内のファイルを検索し、正規表現にマッチするもののうち、サイズが最大のものを取得
            // ※稀にサイズが0のファイルが存在するため
            var cardDatabaseFile = GetLargestCardDatabaseFilePath(cardDatabaseDirectoryPath);

            // マッチするファイルが無いか、ファイルが存在しない場合は例外を投げる
            if (string.IsNullOrEmpty(cardDatabaseFile) || !File.Exists(cardDatabaseFile))
            {
                return "";
            }

            return Path.GetFullPath(cardDatabaseFile);
        }


    }
}
