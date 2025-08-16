using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    internal sealed class Config
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
            public string ProcessName { get; set; } = "MTGA";
            public string LogFileName { get; set; } = "Player.log";
            public string LogDirectoryPath { get; set; } = 
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "Wizards Of The Coast", "MTGA"
            );
            public string CardDatabaseDirectoryPath { get; set; } = 
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Wizards of the Coast", "MTGA", "MTGA_Data", "Downloads", "Raw"
            );
        }
        public MtgArena MtgArenaSettings { get; set; } = new MtgArena();
        
        public class Speaker
        {
            public const string WindowsSpeechAPI = "Windows Speech API";
            public const string VOICEVOX = "VOICEVOX";
            public const string AssistantSeika = "AssistantSeika";
            public const string SpeakModeOn = "on";
            public const string SpeakModeOff = "off";
            public const string SpeakModeThird = "third";
            public const string DefaultSynthesizerName = WindowsSpeechAPI;
            public Dictionary<string, string> SpeakModes { get; set; } = new Dictionary<string, string>
            {
                { PlayerWho.You, SpeakModeOn },
                { PlayerWho.Opponent, SpeakModeOn },
            };
            public Dictionary<string, string> SynthesizerNames { get; set; } = new Dictionary<string, string>
            {
                { PlayerWho.You, WindowsSpeechAPI },
                { PlayerWho.Opponent, WindowsSpeechAPI },
            };
            public Dictionary<string, string> VoiceKeys { get; set; } = new Dictionary<string, string>()
            {
                { PlayerWho.You, "" },
                { PlayerWho.Opponent, "" },
            };
        }
        public Speaker SpeakerSettings { get; set; } = new Speaker();
    }

}
