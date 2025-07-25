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

            // SQLite�f�[�^�t�@�C���̃p�X���w�肵�Ă�������
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

        // TextBox�Ƀ��O��ǉ��iUI�X���b�h�ň��S�Ɏ��s�j
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

        private static string? SelectDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                // �_�C�A���O�̐ݒ�i�I�v�V�����j
                // dialog.Description = "MTG�A���[�i�̃��O�t�@�C�����o�͂����f�B���N�g����I�����Ă�������"; // �_�C�A���O�̐���
                // dialog.ShowNewFolderButton = false; // �V�����t�H���_�쐬�{�^���̕\���i�f�t�H���g: true�j

                // �_�C�A���O��\��
                DialogResult result = dialog.ShowDialog();

                // ���[�U�[���uOK�v���N���b�N�����ꍇ�A�I�����ꂽ�p�X��Ԃ�
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }

                // �L�����Z���܂��͗L���ȃp�X���I������Ȃ������ꍇ
                return null;
            }
        }

    }
}
