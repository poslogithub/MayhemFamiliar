using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            _logWatcer = new LogWatcher(LogToTextBox);
            _ctsLogWatcher = new CancellationTokenSource();
            Task.Run(() => _logWatcer.Start(_ctsLogWatcher.Token));

            _jsonParser = new JsonParser(LogToTextBox);
            _ctsJsonParser = new CancellationTokenSource();
            Task.Run(() => _jsonParser.Start(_ctsJsonParser.Token));

            _dialogueGenerator = new DialogueGenerator(LogToTextBox);
            _ctsDialogueGenerator = new CancellationTokenSource();
            Task.Run(() => _dialogueGenerator.Start(_ctsDialogueGenerator.Token));

            _speaker = new Speaker(LogToTextBox);
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

        private static string? SelectDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                // ダイアログの設定（オプション）
                // dialog.Description = "MTGアリーナのログファイルが出力されるディレクトリを選択してください"; // ダイアログの説明
                // dialog.ShowNewFolderButton = false; // 新しいフォルダ作成ボタンの表示（デフォルト: true）

                // ダイアログを表示
                DialogResult result = dialog.ShowDialog();

                // ユーザーが「OK」をクリックした場合、選択されたパスを返す
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }

                // キャンセルまたは有効なパスが選択されなかった場合
                return null;
            }
        }

    }
}
