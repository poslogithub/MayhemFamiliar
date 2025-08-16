using System;
using System.Collections.Generic;
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
        // UI用
        private Dictionary<string, ComboBox> _comboBoxSynthesizers = new Dictionary<string, ComboBox>();
        private Dictionary<string, Button> _buttonTestSpeaks = new Dictionary<string, Button>();
        private Dictionary<string, ListBox> _listBoxVoices = new Dictionary<string, ListBox>();

        // LogWatcher用
        private const string DetailedLogEnabled = "DETAILED LOGS: ENABLED";
        private string _mtgaLogFilePath;
        // JsonParser用
        private const string CardDatabaseFileNamePattern = @"Raw_CardDatabase_.*\.mtga";
        private string _cardDatabaseFilePath;

        private LogWatcher _logWatcer;
        private JsonParser _jsonParser;
        private DialogueGenerator _dialogueGenerator;
        private Speaker _speaker;
        private CancellationTokenSource _ctsLogWatcher, _ctsJsonParser, _ctsDialogueGenerator, _ctsSpeaker;
        private Boolean _readFullLog = false;

        public MainForm(string[] args)
        {
            InitializeComponent();

            // 引数の処理
            _readFullLog = args.Any(arg => arg == "-f");

            _comboBoxSynthesizers[PlayerWho.You] = comboBoxYourSynthesizer;
            _comboBoxSynthesizers[PlayerWho.Opponent] = comboBoxOpponentSynthesizer;
            _listBoxVoices[PlayerWho.You] = listBoxYourVoices;
            _listBoxVoices[PlayerWho.Opponent] = listBoxOpponentsVoices;
            _buttonTestSpeaks[PlayerWho.You] = buttonYourTestSpeak;
            _buttonTestSpeaks[PlayerWho.Opponent] = buttonOpponentsTestSpeak;

            foreach (var playerWho in new string[] {PlayerWho.You, PlayerWho.Opponent})
            {
                _comboBoxSynthesizers[playerWho].Items.Add(Config.Speaker.WindowsSpeechAPI);
                _comboBoxSynthesizers[playerWho].Items.Add(Config.Speaker.VOICEVOX);
                _comboBoxSynthesizers[playerWho].SelectedIndex = 0; // デフォルトで最初のアイテムを選択
                _comboBoxSynthesizers[playerWho].SelectedIndexChanged += (s, e) =>
                {
                    if (_speaker == null) return;
                    try
                    {
                        string selectedSynthesizer = _comboBoxSynthesizers[playerWho].SelectedItem.ToString();
                        switch (selectedSynthesizer)
                        {
                            case Config.Speaker.WindowsSpeechAPI:
                                _speaker.SetSynthesizer(playerWho, new SpeechAPI());
                                Program._config.SpeakerSettings.SynthesizerNames[playerWho] = Config.Speaker.WindowsSpeechAPI;
                                break;
                            case Config.Speaker.VOICEVOX:
                                _speaker.SetSynthesizer(playerWho, new Voicevox());
                                Program._config.SpeakerSettings.SynthesizerNames[playerWho] = Config.Speaker.VOICEVOX;
                                break;
                            default:
                                Logger.Instance.Log($"{this.GetType().Name}: 不明なシンセサイザー: {selectedSynthesizer}", LogLevel.Error);
                                return;
                        }
                        UpdateVoices(playerWho);
                        _listBoxVoices[playerWho].SelectedIndex = 0; // デフォルトで最初のアイテムを選択
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log($"{this.GetType().Name}: {ex}");
                    }
                };

                _buttonTestSpeaks[playerWho].Click += (s, e) =>
                {
                    if (_speaker == null) return;
                    _speaker.Speak(playerWho, "テスト");
                };

                _listBoxVoices[playerWho].SelectedIndexChanged += (s, e) =>
                {
                    if (_speaker == null) return;
                    if (_listBoxVoices[playerWho].SelectedItem is BoxItem selectedItem)
                    {
                        _speaker.SetVoice(playerWho, selectedItem.Value.ToString());
                        _speaker.InitializeSpeaker(playerWho);
                        Program._config.SpeakerSettings.VoiceKeys[playerWho] = selectedItem.Value.ToString();
                    }
                };

            }

            // 実況モードのツールチップ設定
            ToolTip toolTipSpeakMode = new ToolTip();
            toolTipSpeakMode.SetToolTip(radioButtonYourSpeakModeOn, "「大釜の使い魔をキャスト。」");
            toolTipSpeakMode.SetToolTip(radioButtonOpponentsSpeakModeOn, "「波乱の悪魔をキャスト。」");
            toolTipSpeakMode.SetToolTip(radioButtonOpponentsSpeakModeThird, "「お相手が波乱の悪魔をキャスト。」");

            // イベントハンドラの設定
            this.Shown += Form_Shown;
            this.FormClosing += Form_FormClosing;

            // 実況モード変更のイベントハンドラを設定
            radioButtonYourSpeakModeOn.CheckedChanged += ChangeYourSpeakMode;
            radioButtonYourSpeakModeOff.CheckedChanged += ChangeYourSpeakMode;
            radioButtonOpponentsSpeakModeOn.CheckedChanged += ChangeOpponentsSpeakMode;
            radioButtonOpponentsSpeakModeThird.CheckedChanged += ChangeOpponentsSpeakMode;
            radioButtonOpponentsSpeakModeOff.CheckedChanged += ChangeOpponentsSpeakMode;

            /*
            // シンセサイザー変更のイベントハンドラを設定
            radioButtonYourSAPI.CheckedChanged += ChangeYourSynthesizer;
            radioButtonYourVoicevox.CheckedChanged += ChangeYourSynthesizer;
            radioButtonOpponentsSAPI.CheckedChanged += ChangeOpponentsSynthesizer;
            radioButtonOpponentsVoicevox.CheckedChanged += ChangeOpponentsSynthesizer;
            */
        }


        private void ChangeYourSpeakMode(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;
            if (radioButtonYourSpeakModeOn.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.You] = Config.Speaker.SpeakModeOn;
            }
            else if (radioButtonYourSpeakModeOff.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.You] = Config.Speaker.SpeakModeOff;
            }
        }
        private void ChangeOpponentsSpeakMode(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;
            if (radioButtonOpponentsSpeakModeOn.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] = Config.Speaker.SpeakModeOn;
            }
            else if (radioButtonOpponentsSpeakModeThird.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] = Config.Speaker.SpeakModeThird;
            }
            else if (radioButtonOpponentsSpeakModeOff.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] = Config.Speaker.SpeakModeOff;
            }
        }

        private async void Form_Shown(object sender, EventArgs e)
        {
            // Logger初期化
            Logger.Initialize(LogToTextBox);

            var downloadUrl = await UpdateChecker.CheckForUpdate();
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                DialogResult result = MessageBox.Show(
                    $"新しいバージョンが利用可能です。{Environment.NewLine}ダウンロードページ {downloadUrl} に移動しますか？",
                    $"{Application.ProductName} アップデート確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(downloadUrl);
                }
            }
            
            // コンフィグ読み込み
            Program._config = Config.Load();

            // MTG Arena起動確認
            Logger.Instance.Log($"{this.GetType().Name}: MTG Arena起動確認");
            string mtgaProcessName = Program._config.MtgArenaSettings?.ProcessName ?? DefaultValue.MtgaProcessName;
            while (!Util.IsProcessRunning(mtgaProcessName))
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
            Program._config.MtgArenaSettings.ProcessName = mtgaProcessName;

            // ログファイル存在確認
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
            Program._config.MtgArenaSettings.LogDirectoryPath = Path.GetDirectoryName(_mtgaLogFilePath);

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
            _cardDatabaseFilePath = GetInitCardDatabaseFilePath();
            while (!File.Exists(_cardDatabaseFilePath))
            {
                Logger.Instance.Log($"{this.GetType().Name}: カードデータベースファイル {CardDatabaseFileNamePattern} が {_cardDatabaseFilePath} に見つかりません。", LogLevel.Error);
                DialogResult result = MessageBox.Show(
                    $"カードデータベースファイル {CardDatabaseFileNamePattern} が {_cardDatabaseFilePath} に見つかりませんでした。{Environment.NewLine}次に表示されるダイアログで {CardDatabaseFileNamePattern} が存在するフォルダを選択してください。",
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
            Program._config.MtgArenaSettings.CardDatabaseDirectoryPath = Path.GetDirectoryName(_cardDatabaseFilePath);

            // TODO: ここで実況モードの初期設定
            switch (Program._config.SpeakerSettings?.SpeakModes[PlayerWho.You])
            {
                case Config.Speaker.SpeakModeOn:
                    radioButtonYourSpeakModeOn.Checked = true;
                    break;
                case Config.Speaker.SpeakModeOff:
                    radioButtonYourSpeakModeOff.Checked = true;
                    break;
                default:
                    Logger.Instance.Log($"{this.GetType().Name}: 不明な実況モード: {Program._config.SpeakerSettings?.SpeakModes[PlayerWho.You]}", LogLevel.Error);
                    break;
            }
            switch (Program._config.SpeakerSettings?.SpeakModes[PlayerWho.Opponent])
            {
                case Config.Speaker.SpeakModeOn:
                    radioButtonOpponentsSpeakModeOn.Checked = true;
                    break;
                case Config.Speaker.SpeakModeThird:
                    radioButtonOpponentsSpeakModeThird.Checked = true;
                    break;
                case Config.Speaker.SpeakModeOff:
                    radioButtonOpponentsSpeakModeOff.Checked = true;
                    break;
                default:
                    Logger.Instance.Log($"{this.GetType().Name}: 不明な実況モード: {Program._config.SpeakerSettings?.SpeakModes[PlayerWho.Opponent]}", LogLevel.Error);
                    break;
            }

            // LogWacher起動
            _logWatcer = new LogWatcher(_mtgaLogFilePath);
            _ctsLogWatcher = new CancellationTokenSource();
            Task.Run(() => _logWatcer.Start(_ctsLogWatcher.Token, _readFullLog));

            // JsonParser起動
            _jsonParser = new JsonParser(_cardDatabaseFilePath);
            _ctsJsonParser = new CancellationTokenSource();
            Task.Run(() => _jsonParser.Start(_ctsJsonParser.Token));

            // DialogGenerator起動
            _dialogueGenerator = new DialogueGenerator();
            _ctsDialogueGenerator = new CancellationTokenSource();
            Task.Run(() => _dialogueGenerator.Start(_ctsDialogueGenerator.Token));

            // 自分の初期Speaker決定
            Dictionary<string, ISynthesizer> synthesizers = new Dictionary<string, ISynthesizer>();
            foreach (var playerWho in new string[] { PlayerWho.You, PlayerWho.Opponent })
            {
                string synthesizerName = Program._config.SpeakerSettings?.SynthesizerNames?[playerWho] ?? Config.Speaker.DefaultSynthesizerName;
                string voiceKey = Program._config.SpeakerSettings?.VoiceKeys?[playerWho] ?? "";
                SelectComboBoxItemByText(_comboBoxSynthesizers[playerWho], synthesizerName);
                // ここで_comboBoxSynthesizers[].SelectedIndexChangedが実行されて_configが更新される
                switch (synthesizerName)
                {
                    case Config.Speaker.VOICEVOX:
                        synthesizers[playerWho] = new Voicevox();
                        break;
                    default:
                        synthesizers[playerWho] = new SpeechAPI();
                        break;
                }
            }
            _speaker = new Speaker(synthesizers[PlayerWho.You], synthesizers[PlayerWho.Opponent]);

            _ctsSpeaker = new CancellationTokenSource();
            Task.Run(() => _speaker.Start(_ctsSpeaker.Token));

            foreach (var playerWho in new string[] { PlayerWho.You, PlayerWho.Opponent })
            {
                UpdateVoices(playerWho);    // 話者一覧を更新
                if (!string.IsNullOrEmpty(Program._config.SpeakerSettings.VoiceKeys[playerWho]))
                {
                    // 話者キーが設定されている場合、選択状態にする
                    _listBoxVoices[playerWho].SelectedItem = _listBoxVoices[playerWho].Items.Cast<BoxItem>().FirstOrDefault(item => item.Value.ToString() == Program._config.SpeakerSettings.VoiceKeys[playerWho]);
                    // ここで_comboBoxSynthesizers[].SelectedIndexChangedが実行されて_configが更新される
                }
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program._config.Save();
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
            return Path.Combine(
                Program._config.MtgArenaSettings?.LogDirectoryPath ?? DefaultValue.MtgaLogDirectory, 
                Program._config.MtgArenaSettings?.LogFileName ?? DefaultValue.MtgaLogFileName
            );
        }
        private string GetMtgaLogFilePath()
        {
            string logDirectory;

            if (Directory.Exists(Program._config.MtgArenaSettings?.LogDirectoryPath ?? DefaultValue.MtgaLogDirectory))
            {
                logDirectory = Program._config.MtgArenaSettings?.LogDirectoryPath ?? DefaultValue.MtgaLogDirectory;
            }
            else
            {
                logDirectory = SelectDirectory();
            }
            if (string.IsNullOrEmpty(logDirectory))
            {
                return "";
            }

            return Path.Combine(logDirectory, Program._config.MtgArenaSettings?.LogFileName ?? DefaultValue.MtgaLogFileName);
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
        {
            string cardDatabaseDirectoryPath = Program._config.MtgArenaSettings?.CardDatabaseDirectoryPath ?? DefaultValue.CardDatabaseDirectory;
            var largestCardDatabaseFilePath = GetLargestCardDatabaseFilePath(cardDatabaseDirectoryPath);
            if (!string.IsNullOrEmpty(largestCardDatabaseFilePath) && File.Exists(largestCardDatabaseFilePath))
            {
                return largestCardDatabaseFilePath;
            }
            else
            {
                return "";
            }
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
            string cardDatabaseDirectoryPath = Program._config.MtgArenaSettings?.CardDatabaseDirectoryPath ?? DefaultValue.CardDatabaseDirectory;
            if (!Directory.Exists(cardDatabaseDirectoryPath))
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

        private void UpdateVoices(string playerWho)
        {
            switch (playerWho)
            {
                case PlayerWho.You:
                    listBoxYourVoices.Items.Clear();
                    List<IVoice> yourVoices = _speaker?.GetVoices(PlayerWho.You) ?? new List<IVoice>();
                    foreach (var voice in yourVoices)
                    {
                        listBoxYourVoices.Items.Add(new BoxItem { Label = voice.GetLabel(), Value = voice.GetKey() });
                    }
                    break;
                case PlayerWho.Opponent:
                    listBoxOpponentsVoices.Items.Clear();
                    List<IVoice> opponentsVoices = _speaker?.GetVoices(PlayerWho.Opponent) ?? new List<IVoice>();
                    foreach (var voice in opponentsVoices)
                    {
                        listBoxOpponentsVoices.Items.Add(new BoxItem { Label = voice.GetLabel(), Value = voice.GetKey() });
                    }
                    break;
                default:
                    Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤー: {playerWho}", LogLevel.Error);
                    break;
            }
        }

        private void SelectComboBoxItemByText(ComboBox comboBox, string text)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == text)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
            comboBox.SelectedIndex = -1; // 一致なし
        }
    }

    public class BoxItem
    {
        public string Label { get; set; }
        public object Value { get; set; }
        public override string ToString() => Label; // ListBox表示用
    }
}
