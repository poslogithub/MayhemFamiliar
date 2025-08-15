using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MayhemFamiliar
{
    internal class Speaker
    {
        private Dictionary<string, ISynthesizer> _synthesizers = new Dictionary<string, ISynthesizer>();
        public Speaker(ISynthesizer synthesizerYou, ISynthesizer synthesizerOpponent)
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
                    if (DialogueQueue.Queue.TryDequeue(out string dialogue))
                    {
                        if (dialogue.StartsWith(PlayerWho.You))
                        {
                            if (Program._config.SpeakerSettings.SpeakModes[PlayerWho.You] == Config.Speaker.SpeakModeOff)
                            {
                                Logger.Instance.Log($"{this.GetType().Name}: {PlayerWho.You} の発話モードがオフのため、スキップ: {dialogue}");
                                continue;
                            }
                            dialogue = Regex.Replace(dialogue, $"^{PlayerWho.You} ", "");
                            _synthesizers[PlayerWho.You].ProcessDialogue(dialogue);
                        }
                        else if (dialogue.StartsWith(PlayerWho.Opponent))
                        {
                            if (Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] == Config.Speaker.SpeakModeOff)
                            {
                                Logger.Instance.Log($"{this.GetType().Name}: {PlayerWho.Opponent} の発話モードがオフのため、スキップ: {dialogue}");
                                continue;
                            }
                            dialogue = Regex.Replace(dialogue, $"^{PlayerWho.Opponent} ", "");
                            _synthesizers[PlayerWho.Opponent].ProcessDialogue(dialogue);
                        }
                        else
                        {
                            Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤー: {dialogue}");
                        }
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
        public List<IVoice> GetVoices(string playerWho)
        {
            return _synthesizers[playerWho].GetVoices();
        }
        public void SetVoice(string playerWho, string voiceName)
        {
            _synthesizers[playerWho].SetVoice(voiceName);
        }
        public void Speech(string playerWho, string dialogue)
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
    internal class SpeechAPI : ISynthesizer
    {
        private SpeechSynthesizer _synthesizer;
        public SpeechAPI()
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
            foreach (var voice in _synthesizer.GetInstalledVoices(CultureInfo.CurrentCulture))
            {
                voices.Add(new SpeechAPIVoice(voice.VoiceInfo.Name, voice.VoiceInfo.Description));
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
        string Name { get; set; }
        string Description { get; set; }
        string StyleName { get; set; }
        int StyleID { get; set; }
        string GetKey();
        string GetLabel();
        string GetImplementation();
    }

    internal class VoicevoxVoice : IVoice
    {
        public string Name { get; set; }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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

    internal class SpeechAPIVoice : IVoice
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string StyleName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int StyleID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public SpeechAPIVoice(string name, string description)
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
            return Config.Speaker.SpeechAPI;
        }
    }
}
