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

            // SQLite�f�[�^�t�@�C���̃p�X���w�肵�Ă�������
            // string dbFilePath = @"C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw\Raw_CardDatabase_96e0ea3603a307b5cd9a2de7f4dbcafc.mtga";
            // _cardData = new CardData(dbFilePath);
            Logger.Initialize(LogToTextBox);

            try
            {
                _logFilePath = GetLogFilePath();
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ���O�f�B���N�g�� {ex.Message} ��������܂���", LogLevel.Error);
                MessageBox.Show($"���O�f�B���N�g�� {ex.Message} ��������܂���B�A�v���P�[�V�������I�����܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            catch (FileNotFoundException ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ���O�t�@�C�� {ex.Message} ��������܂���: {ex.Message}", LogLevel.Error);
                MessageBox.Show("���O�t�@�C����������܂���B�A�v���P�[�V�������I�����܂��B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            Logger.Instance.Log($"{this.GetType().Name}: ���O�t�@�C��: {_logFilePath}");

            // �ڍ׃��O�L���m�F
            if (CheckDetailedLog())
            {
                Logger.Instance.Log($"{this.GetType().Name}: �ڍ׃��O���L��");
            }
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: �ڍ׃��O������");
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

        private string GetLogFilePath()
        {
            string logDirectory;
            // ���O�f�B���N�g���m��
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

            // ���O�t�@�C���m��
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
                // �_�C�A���O�̐ݒ�i�I�v�V�����j
                dialog.ShowNewFolderButton = false; // �V�����t�H���_�쐬�{�^���̕\���i�f�t�H���g: true�j

                // �_�C�A���O��\��
                DialogResult result = dialog.ShowDialog();

                // ���[�U�[���uOK�v���N���b�N�����ꍇ�A�I�����ꂽ�p�X��Ԃ�
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }

                // �L�����Z���܂��͗L���ȃp�X���I������Ȃ������ꍇ
                return "";
            }
        }
        private Boolean CheckDetailedLog()
        {
            try
            {
                // �t�@�C���T�C�Y���擾
                var fileInfo = new FileInfo(_logFilePath);
                // �ǋL������ǂݍ���
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
                Console.WriteLine($"{this.GetType().Name}: �ڍ׃��O�L���m�F�ŗ�O������");
                Console.WriteLine($"{this.GetType().Name}: �ǂݍ��݃G���[: {ex.Message}");
            }
            return false;
        }

    }
}
