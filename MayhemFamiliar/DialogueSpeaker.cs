using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class DialogueSpeaker
    {
        private Dictionary<string, ISynthesizer> _synthesizers = new Dictionary<string, ISynthesizer>();
        public DialogueSpeaker(ISynthesizer synthesizerYou, ISynthesizer synthesizerOpponent)
        {
            _synthesizers[PlayerWho.You] = synthesizerYou;
            _synthesizers[PlayerWho.Opponent] = synthesizerOpponent;
        }
        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                Logger.Instance.Log($"{this.GetType().Name}: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (DialogueQueue.Queue.TryDequeue(out Dialogue dialogue))
                    {
                        ProcessDialogue(dialogue);
                    }
                    else
                    {
                        // キューが空なら短い間隔で待機（ブロック）
                        await Task.Delay(100, cancellationToken);
                    }
                }
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (OperationCanceledException)
            {
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: エラー発生: {ex.Message}");
            }
        }
        private void ProcessDialogue(Dialogue dialogue)
        {
            string playerWho = dialogue.PlayerWho;

            if (playerWho != PlayerWho.Unknown)
            { 
                if (Program._config.SpeakerSettings.SpeakModes[playerWho] != Config.Speaker.SpeakModeOff)
                {
                    _synthesizers[playerWho].ProcessDialogue(dialogue.Content);
                }
                if (Program._config.YukaConneNEOSettings.Enabled)
                {
                    Task.Run(() => SendTextToYukaConneNEOAsync(dialogue.Content));
                }
            }
            else
            {
                Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤー: {dialogue}");
            }
        }
        public async Task SendTextToYukaConneNEOAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            string url = $"http://127.0.0.1:{Program._config.YukaConneNEOSettings.Port}/api/input?text={Uri.EscapeDataString(text)}";
            Logger.Instance.Log($"{typeof(YukaConneNEO).Name}: ゆかコネNEOにテキスト送信: {text}");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Log($"{typeof(YukaConneNEO).Name}: ゆかコネNEOがエラー応答: {url} {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{typeof(YukaConneNEO).Name}: ゆかコネNEOへのテキスト送信が失敗: {url} {ex.Message}");
            }
        }
        public List<IVoice> GetVoices(string playerWho)
        {
            return _synthesizers[playerWho].GetVoices();
        }
        public void SetVoice(string playerWho, string voiceName)
        {
            _synthesizers[playerWho].SetVoice(voiceName);
        }
        public void Speak(string playerWho, string dialogue)
        {
            _synthesizers[playerWho].ProcessDialogue(dialogue);
        }
        public void InitializeSpeaker(string playerWho)
        {
            _synthesizers[playerWho].InitializeSpeaker();
        }
        public void SetSynthesizer(string playerWho, ISynthesizer synthesizer)
        {
            _synthesizers[playerWho] = synthesizer;
            Logger.Instance.Log($"{this.GetType().Name}: {playerWho} の音声合成器を設定: {synthesizer.GetType().Name}");
        }
    }

    internal interface ISynthesizer
    {
        void ProcessDialogue(string dialogue);
        List<IVoice> GetVoices();
        void SetVoice(string key);
        void InitializeSpeaker();
    }

    internal class Voicevox : ISynthesizer
    {
        private const string BaseUrl = "http://127.0.0.1:50021/";
        private int _styleId = -1;

        public void ProcessDialogue(string dialogue)
        {
            if (_styleId < 0)
            {
                Logger.Instance.Log("VoicevoxAPI: 話者が設定されていません", LogLevel.Error);
                return;
            }

            try
            {
                // 1. audio_query
                var queryUrl = $"{BaseUrl}audio_query?text={Uri.EscapeDataString(dialogue)}&speaker={_styleId}";
                var queryRequest = (HttpWebRequest)WebRequest.Create(queryUrl);
                queryRequest.Method = "POST";
                queryRequest.Accept = "application/json";
                queryRequest.ContentLength = 0; // POSTだがbodyなし

                string queryJson;
                using (var queryResponse = (HttpWebResponse)queryRequest.GetResponse())
                using (var queryStream = queryResponse.GetResponseStream())
                using (var queryReader = new StreamReader(queryStream))
                {
                    queryJson = queryReader.ReadToEnd();
                }

                // 2. synthesis
                var synthUrl = $"{BaseUrl}synthesis?speaker={_styleId}";
                var synthRequest = (HttpWebRequest)WebRequest.Create(synthUrl);
                synthRequest.Method = "POST";
                synthRequest.Accept = "audio/wav";
                synthRequest.ContentType = "application/json";

                using (var synthStream = synthRequest.GetRequestStream())
                using (var writer = new StreamWriter(synthStream))
                {
                    writer.Write(queryJson);
                }

                using (var synthResponse = (HttpWebResponse)synthRequest.GetResponse())
                using (var audioStream = synthResponse.GetResponseStream())
                {
                    // 一時ファイルに保存
                    var tempFile = Path.GetTempFileName() + ".wav";
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        audioStream.CopyTo(fileStream);
                    }

                    // 再生（System.Media.SoundPlayerを利用）
                    using (var player = new System.Media.SoundPlayer(tempFile))
                    {
                        Logger.Instance.Log($"{this.GetType().Name}: 発話: {dialogue}");
                        player.PlaySync();
                    }

                    // 一時ファイル削除
                    File.Delete(tempFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"VoicevoxAPI: 音声合成失敗: {ex.Message}", LogLevel.Error);
            }
        }

        public List<IVoice> GetVoices()
        {
            var voices = new List<IVoice>();
            try
            {
                var url = $"{BaseUrl}speakers";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var arr = JArray.Parse(json);
                    foreach (var speaker in arr)
                    {
                        var styles = speaker["styles"] as JArray;
                        foreach (var style in styles)
                        {
                            string name = speaker["name"]?.ToString();
                            string styleName = style["name"]?.ToString();
                            int styleId = style["id"]?.ToObject<int>() ?? 0;
                            voices.Add(new VoicevoxVoice(name, styleName, styleId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"VoicevoxAPI: VOICEVOX話者一覧取得失敗: {ex.Message}", LogLevel.Error);
            }
            return voices;
        }

        public void InitializeSpeaker()
        {
            var voices = new List<IVoice>();
            try
            {
                var url = $"{BaseUrl}initialize_speaker?speaker={_styleId}&skip_reinit=true";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Accept = "*/*";
                request.ContentLength = 0; // POSTだがbodyなし

                string queryJson;
                using (var queryResponse = (HttpWebResponse)request.GetResponse())
                using (var queryStream = queryResponse.GetResponseStream())
                using (var queryReader = new StreamReader(queryStream))
                {
                    queryJson = queryReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"VoicevoxAPI: VOICEVOX話者一覧取得失敗: {ex.Message}", LogLevel.Error);
            }
        }
        public void SetVoice(string key)
        {
            _styleId = int.Parse(key);
            Logger.Instance.Log($"{this.GetType().Name}: 話者設定: {_styleId}");

        }

    }

    internal class AssistantSeika : ISynthesizer
    {
        private string SeikaSay2ExePath = Path.Combine(
            ".", 
            Program._config.SpeakerSettings?.SeikaSay2Exe ?? DefaultValue.SeikaSay2Exe
        );
        private string _cid = "";
        public void ProcessDialogue(string dialogue)
        {
            if (string.IsNullOrEmpty(_cid))
            {
                Logger.Instance.Log($"{this.GetType().Name}: 話者が設定されていません", LogLevel.Error);
                return;
            }
            Process process = new Process();
            process.StartInfo.FileName = SeikaSay2ExePath;
            process.StartInfo.Arguments = $"-cid {_cid} -t {dialogue}";
            process.StartInfo.UseShellExecute = false; // シェルを介さず直接実行
            process.StartInfo.CreateNoWindow = true; // ウィンドウを表示しない
            try
            {
                process.Start();
                Logger.Instance.Log($"{this.GetType().Name}: 発話: {dialogue}");
                process.WaitForExit(); // プロセスの終了を待機
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {process.StartInfo.FileName} {process.StartInfo.Arguments} 実行時に例外が発生");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
            }

        }
        public List<IVoice> GetVoices()
        {
            Logger.Instance.Log($"{this.GetType().Name}: 音声一覧を取得");
            Process process = new Process();
            process.StartInfo.FileName = SeikaSay2ExePath;
            process.StartInfo.Arguments = $"-list";
            process.StartInfo.UseShellExecute = false; // シェルを介さず直接実行
            process.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
            process.StartInfo.RedirectStandardError = true; // エラー出力もリダイレクト（必要に応じて）
            process.StartInfo.CreateNoWindow = true; // ウィンドウを表示しない
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {process.StartInfo.FileName} {process.StartInfo.Arguments} 実行時に例外が発生");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
            }

            List<string> cids = new List<string>();
            List<string> speakers = new List<string>();
            var voices = new List<IVoice>();
            try
            {
                StreamReader reader = process.StandardOutput;
                string line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    if (Regex.IsMatch(line, "^[0-9]"))
                    {
                        voices.Add(new AssistantSeikaVoice(line.Split(' ')[0], line));
                    }
                    line = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {process.StartInfo.FileName} の標準出力読み込みで例外が発生");
                Logger.Instance.Log($"{this.GetType().Name}: {process.StartInfo.ToString()}");
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}");
            }
            return voices;
        }
        public void SetVoice(string cid)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 話者設定: {cid}");
            _cid = cid;
        }
        public void InitializeSpeaker() { }
    }

    internal class WindowsSpeechAPI : ISynthesizer
    {
        private SpeechSynthesizer _synthesizer;
        public WindowsSpeechAPI()
        {
            _synthesizer = new SpeechSynthesizer();
        }
        public void ProcessDialogue(string dialogue)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 発話: {dialogue}");
            _synthesizer.Speak(dialogue);
        }
        public List<IVoice> GetVoices()
        {
            Logger.Instance.Log($"{this.GetType().Name}: 音声一覧を取得");
            var voices = new List<IVoice>();
            foreach (InstalledVoice voice in _synthesizer.GetInstalledVoices(CultureInfo.CurrentCulture))
            {
                voices.Add(new WindowsSpeechAPIVoice(voice.VoiceInfo.Name, voice.VoiceInfo.Description));
            }
            return voices;
        }
        public void SetVoice(string key)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 話者設定: {key}");
            _synthesizer.SelectVoice(key);
        }
        public void InitializeSpeaker() { }
    }

    internal interface IVoice
    {
        string GetKey();
        string GetLabel();
        string GetImplementation();
    }

    internal class VoicevoxVoice : IVoice
    {
        public string Name { get; set; }
        public string StyleName { get; set; }
        public int StyleID { get; set; }
        public VoicevoxVoice(string name, string styleName, int styleId)
        {
            Name = name;
            StyleName = styleName;
            StyleID = styleId;
        }
        public string GetKey()
        {
            return StyleID.ToString();
        }
        public string GetLabel()
        {
            return $"{Name} - {StyleName}";
        }
        public string GetImplementation()
        {
            return Config.Speaker.VOICEVOX;
        }
    }

    internal class AssistantSeikaVoice : IVoice
    {
        public string CID { get; set; }
        public string Speaker { get; set; }
        public AssistantSeikaVoice(string cid, string speaker)
        {
            CID = cid;
            Speaker = speaker;
        }
        public string GetKey()
        {
            return CID;
        }
        public string GetLabel()
        {
            return Speaker;
        }
        public string GetImplementation()
        {
            return Config.Speaker.AssistantSeika;
        }
    }

    internal class WindowsSpeechAPIVoice : IVoice
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public WindowsSpeechAPIVoice(string name, string description)
        {
            Name = name;
            Description = description;
        }
        public string GetKey()
        {
            return Name;
        }
        public string GetLabel()
        {
            return Description;
        }
        public string GetImplementation()
        {
            return Config.Speaker.WindowsSpeechAPI;
        }
    }
}
