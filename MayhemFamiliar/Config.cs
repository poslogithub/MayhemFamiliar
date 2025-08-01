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
                return new Config { };
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFileName));
        }
        public static void Save(Config config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFileName, json);
        }
        public string AppName { get; set; }
        public int Version { get; set; }
        public DatabaseConfig Database { get; set; } // ネストしたオブジェクト
        public List<UserConfig> Users { get; set; } // ネストしたリスト
    }

    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }
        public int Timeout { get; set; }
    }

    public class UserConfig
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
