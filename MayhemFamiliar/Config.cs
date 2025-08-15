using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    public sealed class Config
    {
        private static readonly string ConfigFileName = $"{Application.ProductName}.config.json";
        public static Config Load()
        {
            if (!File.Exists(ConfigFileName)) return new Config();

            try
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFileName));
            }
            catch
            {
                return new Config();
            }
        }
        public Boolean Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFileName, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public class MtgArena
        {
            public string ProcessName { get; set; }
            public string LogDirectoryPath { get; set; }
            public string CardDatabaseDirectoryPath { get; set; }
        }
        public MtgArena MtgArenaSettings { get; set; } = new MtgArena();
        
        public class Speaker
        {
            public const string SpeechAPI = "SpeechAPI";
            public const string VOICEVOX = "VOICEVOX";
            public const string SpeakModeOn = "on";
            public const string SpeakModeOff = "off";
            public const string SpeakModeThird = "third";
            public Dictionary<string, string> SpeakModes { get; set; } = new Dictionary<string, string>
            {
                { PlayerWho.You, SpeakModeOn },
                { PlayerWho.Opponent, SpeakModeOn },
            };
            public string YourSynthesizerName { get; set; }
            public string OpponentsSynthesizerName { get; set; }
            public string YourVoiceKey { get; set; }
            public string OpponentsVoiceKey { get; set; }
        }
        public Speaker SpeakerSettings { get; set; } = new Speaker();
    }

}
