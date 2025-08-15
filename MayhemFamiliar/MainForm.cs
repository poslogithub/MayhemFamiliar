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
            _readFullLog = args.Any(arg => arg == "-f");
            InitializeComponent();
            this.Shown += Form_Shown;
            this.FormClosing += Form_FormClosing;
            listBoxYourVoices.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxYourVoices.SelectedItem is ListBoxItem selectedItem && _speaker != null)
                {
                    _speaker.SetVoice(PlayerWho.You, selectedItem.Value.ToString());
                    _speaker.InitializeSpeaker(PlayerWho.You);
                    Program._config.SpeakerSettings.YourVoiceKey = selectedItem.Value.ToString();
                }
            };
            listBoxOpponentsVoices.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxOpponentsVoices.SelectedItem is ListBoxItem selectedItem && _speaker != null)
                {
                    _speaker.SetVoice(PlayerWho.Opponent, selectedItem.Value.ToString());
                    _speaker.InitializeSpeaker(PlayerWho.Opponent);
                    Program._config.SpeakerSettings.OpponentsVoiceKey = selectedItem.Value.ToString();
                }
            };

            buttonYourTestSpeech.Click += ButtonYourTestSpeech_Click;
            buttonOpponentsTestSpeech.Click += ButtonOpponentsTestSpeech_Click;

            // 実況モードのツールチップ設定
            ToolTip toolTipSpeakMode = new ToolTip();
            toolTipSpeakMode.SetToolTip(radioButtonYourSpeakModeOn, "「大釜の使い魔をキャスト。」");
            toolTipSpeakMode.SetToolTip(radioButtonOpponentsSpeakModeOn, "「波乱の悪魔をキャスト。」");
            toolTipSpeakMode.SetToolTip(radioButtonOpponentsSpeakModeThird, "「お相手が波乱の悪魔をキャスト。」");

            // 実況モード変更のイベントハンドラを設定
            radioButtonYourSpeakModeOn.CheckedChanged += ChangeYourSpeakMode;
            radioButtonYourSpeakModeOff.CheckedChanged += ChangeYourSpeakMode;
            radioButtonOpponentsSpeakModeOn.CheckedChanged += ChangeOpponentsSpeakMode;
            radioButtonOpponentsSpeakModeThird.CheckedChanged += ChangeOpponentsSpeakMode;
            radioButtonOpponentsSpeakModeOff.CheckedChanged += ChangeOpponentsSpeakMode;

            // シンセサイザー変更のイベントハンドラを設定
            radioButtonYourSAPI.CheckedChanged += ChangeYourSynthesizer;
            radioButtonYourVoicevox.CheckedChanged += ChangeYourSynthesizer;
            radioButtonOpponentsSAPI.CheckedChanged += ChangeOpponentsSynthesizer;
            radioButtonOpponentsVoicevox.CheckedChanged += ChangeOpponentsSynthesizer;
        }

        private void ChangeYourSpeakMode(object sender, EventArgs e)
        {
            if (radioButtonYourSpeakModeOn.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.You] = Config.Speaker.SpeakModeOn;
            }
            else if (radioButtonYourSpeakModeOff.Checked)
            {
                Program._config.SpeakerSettings.SpeakModes[PlayerWho.You] = Config.Speaker.SpeakModeOff;
            }
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: 不明な実況モード: {Program._config.SpeakerSettings.SpeakModes[PlayerWho.You]}", LogLevel.Error);
            }
        }
        private void ChangeOpponentsSpeakMode(object sender, EventArgs e)
        {
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
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: 不明な実況モード: {Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent]}", LogLevel.Error);
            }
        }
        private void ChangeYourSynthesizer(object sender, EventArgs e)
        {
            if (_speaker == null)
            {
                Logger.Instance.Log($"{this.GetType().Name}: Speakerが初期化されていません。");
                return;
            }
            try
            {
                if (radioButtonYourSAPI.Checked)
                {
                    _speaker.SetSynthesizer(PlayerWho.You, new SpeechAPI());
                    Program._config.SpeakerSettings.YourSynthesizerName = Config.Speaker.SpeechAPI;
                }
                if (radioButtonYourVoicevox.Checked)
                {
                    _speaker.SetSynthesizer(PlayerWho.You, new Voicevox());
                    Program._config.SpeakerSettings.YourSynthesizerName = Config.Speaker.VOICEVOX;
                }
                UpdateVoices(PlayerWho.You);
                listBoxYourVoices.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {ex}");
            }
        }
        private void ChangeOpponentsSynthesizer(object sender, EventArgs e)
        {
            if (_speaker == null)
            {
                Logger.Instance.Log($"{this.GetType().Name}: Speakerが初期化されていません。");
                return;
            }
            try
            {
                if (radioButtonOpponentsSAPI.Checked)
                {
                    _speaker.SetSynthesizer(PlayerWho.Opponent, new SpeechAPI());
                    Program._config.SpeakerSettings.OpponentsSynthesizerName = Config.Speaker.SpeechAPI;
                }
                if (radioButtonOpponentsVoicevox.Checked)
                {
                    _speaker.SetSynthesizer(PlayerWho.Opponent, new Voicevox());
                    Program._config.SpeakerSettings.OpponentsSynthesizerName = Config.Speaker.VOICEVOX;
                }
                UpdateVoices(PlayerWho.Opponent);
                 listBoxOpponentsVoices.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {ex}");
            }
        }

        private void ButtonYourTestSpeech_Click(object sender, EventArgs e)
        {
            _speaker.Speech(PlayerWho.You, "テスト");
        }
        private void ButtonOpponentsTestSpeech_Click(object sender, EventArgs e)
        {
            _speaker.Speech(PlayerWho.Opponent, "テスト");
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
            string synthesizerName = Program._config.SpeakerSettings?.YourSynthesizerName ?? DefaultValue.synthesizerName;
            string yourVoiceKey = Program._config.SpeakerSettings?.YourVoiceKey ?? "";
            ISynthesizer yourSynthesizer;
            switch (synthesizerName)
            {
                case Config.Speaker.VOICEVOX:
                    radioButtonYourVoicevox.Checked = true;
                    yourSynthesizer = new Voicevox();
                    break;
                default:
                    radioButtonYourSAPI.Checked = true;
                    yourSynthesizer = new SpeechAPI();
                    break;
            }

            // 対戦相手の初期Speaker決定
            synthesizerName = Program._config.SpeakerSettings?.OpponentsSynthesizerName ?? DefaultValue.synthesizerName;
            string opponentsVoiceKey = Program._config.SpeakerSettings?.OpponentsVoiceKey ?? "";
            ISynthesizer opponentsSynthesizer;
            switch (synthesizerName)
            {
                case Config.Speaker.VOICEVOX:
                    radioButtonOpponentsVoicevox.Checked = true;
                    opponentsSynthesizer = new Voicevox();
                    break;
                default:
                    radioButtonOpponentsSAPI.Checked = true;
                    opponentsSynthesizer = new SpeechAPI();
                    break;
            }
            _speaker = new Speaker(yourSynthesizer, opponentsSynthesizer);

            _ctsSpeaker = new CancellationTokenSource();
            Task.Run(() => _speaker.Start(_ctsSpeaker.Token));

            // 話者一覧を表示
            UpdateVoices(PlayerWho.You);
            if (!string.IsNullOrEmpty(yourVoiceKey))
            {
                // TODO: ここでConfigの話者を読み込む
                listBoxYourVoices.SelectedItem = listBoxYourVoices.Items.Cast<ListBoxItem>().FirstOrDefault(item => item.Value.ToString() == yourVoiceKey);
            }
            UpdateVoices(PlayerWho.Opponent);
            if (!string.IsNullOrEmpty(opponentsVoiceKey))
            {
                // TODO: ここでConfigの話者を読み込む
                listBoxOpponentsVoices.SelectedItem = listBoxOpponentsVoices.Items.Cast<ListBoxItem>().FirstOrDefault(item => item.Value.ToString() == opponentsVoiceKey);
            }
            // 自分の話者を設定
            Program._config.SpeakerSettings.YourSynthesizerName = synthesizerName;
            var selectedItem = listBoxYourVoices.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                _speaker?.SetVoice(PlayerWho.You, selectedItem.Value.ToString());
                Program._config.SpeakerSettings.YourVoiceKey = selectedItem.Value.ToString();
            }
            // 対戦相手の話者を設定
            Program._config.SpeakerSettings.OpponentsSynthesizerName = synthesizerName;
            selectedItem = listBoxOpponentsVoices.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                _speaker?.SetVoice(PlayerWho.Opponent, selectedItem.Value.ToString());
                Program._config.SpeakerSettings.OpponentsVoiceKey = selectedItem.Value.ToString();
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
            return Path.Combine(Program._config.MtgArenaSettings?.LogDirectoryPath ?? DefaultValue.MtgaLogDirectory, DefaultValue.MtgaLogFileName);
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
            var cardDatabaseDirectoryPath = Program._config.MtgArenaSettings?.CardDatabaseDirectoryPath;
            var largestCardDatabaseFilePath = GetLargestCardDatabaseFilePath(cardDatabaseDirectoryPath);
            if (!string.IsNullOrEmpty(largestCardDatabaseFilePath) && File.Exists(largestCardDatabaseFilePath))
            {
                return largestCardDatabaseFilePath;
            }
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

        private void UpdateVoices(string playerWho)
        {
            switch (playerWho)
            {
                case PlayerWho.You:
                    listBoxYourVoices.Items.Clear();
                    List<IVoice> yourVoices = _speaker?.GetVoices(PlayerWho.You) ?? new List<IVoice>();
                    foreach (var voice in yourVoices)
                    {
                        listBoxYourVoices.Items.Add(new ListBoxItem { Label = voice.GetLabel(), Value = voice.GetKey() });
                    }
                    break;
                case PlayerWho.Opponent:
                    listBoxOpponentsVoices.Items.Clear();
                    List<IVoice> opponentsVoices = _speaker?.GetVoices(PlayerWho.Opponent) ?? new List<IVoice>();
                    foreach (var voice in opponentsVoices)
                    {
                        listBoxOpponentsVoices.Items.Add(new ListBoxItem { Label = voice.GetLabel(), Value = voice.GetKey() });
                    }
                    break;
                default:
                    Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤー: {playerWho}", LogLevel.Error);
                    break;
            }
        }

    }

    public class ListBoxItem
    {
        public string Label { get; set; }
        public object Value { get; set; }
        public override string ToString() => Label; // ListBox表示用
    }
}
