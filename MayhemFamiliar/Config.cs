using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    public class Config
    {
        private static readonly string ConfigFileName = $"{Application.ProductName}.config.json";
        public static Config Load()
        {
            if (!File.Exists(ConfigFileName))
            {
                return new Config();
            }

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
        public MtgaConfig Mtga { get; set; } = new MtgaConfig();
        public SpeakerConfig Speaker { get; set; } = new SpeakerConfig();
    }

    public class SpeakerConfig
    {
        public const string SpeechAPI = "SpeechAPI";
        public const string VOICEVOX = "VOICEVOX";
        public string synthesizerName { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public string ID { get; set; }
    }
    public class MtgaConfig
    {
        public string ProcessName { get; set; }
        public string LogDirectoryPath { get; set; }
        public string CardDatabaseDirectoryPath { get; set; }
    }
}
