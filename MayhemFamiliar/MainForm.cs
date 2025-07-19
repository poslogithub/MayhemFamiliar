using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MayhemFamiliar
{
    public partial class MainForm : Form
    {
        private CardData? _cardData;
        private LogWatcher? _logTailer;
        private JsonParser? _jsonParser;
        private CancellationTokenSource? _ctsLogTailer, _ctsJsonParser;

        public MainForm()
        {
            InitializeComponent();
            this.Shown += Form_Shown;
            this.FormClosing += Form_FormClosing;
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            var configJson = JObject.Parse(@"{""Config"": { ""MtgaDataDirPath"": ""C:\\Program Files\\Wizards of the Coast\\MTGA\\MTGA_Data\\Downloads\\Raw\\Raw_CardDatabase_96e0ea3603a307b5cd9a2de7f4dbcafc.mtga"", ""MtgaLogDirPath"": ""C:\\Program Files\\Wizards of the Coast\\MTGA\\MTGA_Data\\Logs\\Logs"" } }");

            // SQLiteデータファイルのパスを指定してください
            string dbFilePath = @"C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw\Raw_CardDatabase_96e0ea3603a307b5cd9a2de7f4dbcafc.mtga";
            _cardData = new CardData(dbFilePath);

            _logTailer = new LogWatcher(LogToTextBox);
            _ctsLogTailer = new CancellationTokenSource();
            _logTailer.Start();

            _jsonParser = new JsonParser(LogToTextBox);
            _ctsJsonParser = new CancellationTokenSource();
            _jsonParser.RunAsync(_ctsJsonParser.Token);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ctsLogTailer?.Cancel();
            _ctsLogTailer?.Dispose();
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
                textBoxLog.Invoke(() => textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}"));
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
            else
            {
                textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.ScrollToCaret();
            }
        }
    }
}
