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

            // SQLite�f�[�^�t�@�C���̃p�X���w�肵�Ă�������
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

        // TextBox�Ƀ��O��ǉ��iUI�X���b�h�ň��S�Ɏ��s�j
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
