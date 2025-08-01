using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class Speaker
    {
        private SpeechAPI _api;
        public Speaker(SpeechAPI api)
        {
            _api = api;
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
                        _api.ProcessDialogue(dialogue);
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
        public List<IVoice> GetVoices()
        {
            return _api.GetVoices();
        }
        public void SetVoice(string voiceName)
        {
            _api.SetVoice(voiceName);
        }
        public void Speech(string dialogue)
        {
            _api.ProcessDialogue(dialogue);
        }
    }

    internal interface ISynthesizer
    {
        void ProcessDialogue(string dialogue);
        List<IVoice> GetVoices();
        void SetVoice(string voiceName);
        void SetVoice(int styleId);
    }

    internal class VoicevoxAPI : ISynthesizer
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
                var url = $"{BaseUrl}/speakers";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
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

        public void SetVoice(string voiceName)
        {
            // 実装は省略
            throw new NotImplementedException();
        }

        public void SetVoice(int styleId)
        {
            _styleId = styleId;
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
            Logger.Instance.Log($"{this.GetType().Name}: ダイアログを処理: {dialogue}", LogLevel.Debug);
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
        public void SetVoice(string voiceName)
        {
            Logger.Instance.Log($"{this.GetType().Name}: 音声を設定: {voiceName}");
            _synthesizer.SelectVoice(voiceName);
        }

        public void SetVoice(int styleId)
        {
            throw new NotImplementedException();
        }
    }

    internal interface IVoice
    {
        string Name { get; set; }
        string Description { get; set; }
        string ID { get; set; }
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
        public string ID { get; set; }
        public string StyleName { get; set; }
        public int StyleID { get; set; }
        public VoicevoxVoice(string name, string styleName, int styleId)
        {
            Name = name;
            StyleName = name;
            StyleID = styleId;
        }
        public string GetKey()
        {
            return $"{StyleID}";
        }
        public string GetLabel()
        {
            return $"{Name} - {StyleName}";
        }
        public string GetImplementation()
        {
            return SpeakerConfig.VOICEVOX;
        }
    }

    internal class SpeechAPIVoice : IVoice
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
            return SpeakerConfig.SpeechAPI;
        }
    }
}
