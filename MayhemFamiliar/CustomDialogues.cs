using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MayhemFamiliar
{
    internal class CustomDialogues
    {
        private const string Mode = "mode";
        private const string Text = "text";
        public Dictionary<string, CustomDialogue> Map { get; }
        public CustomDialogues()
        {
            Map = new Dictionary<string, CustomDialogue>();
        }
        public CustomDialogues(string customDialoguesFilePath)
        {
            if (string.IsNullOrEmpty(customDialoguesFilePath))
                throw new ArgumentException($"{this.GetType()} のコンストラクタにnullまたは空文字列が渡されました。", nameof(customDialoguesFilePath));
            if (!File.Exists(customDialoguesFilePath))
                throw new FileNotFoundException($"{this.GetType()} のコンストラクタに渡されたファイルパスが存在しません。", customDialoguesFilePath);

            string jsonString;
            try
            {
                jsonString = File.ReadAllText(customDialoguesFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"{this.GetType()} のコンストラクタに渡されたJSONファイルの読み込みに失敗しました。", ex);
            }

            JObject jsonObject;
            try
            { 
                jsonObject = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                throw new JsonException($"{this.GetType()} のコンストラクタに渡されたJSONファイルのパースに失敗しました。", ex);
            }

            foreach (JProperty property in jsonObject.Properties())
            {
                string name = property.Name;
                JToken token = property.Value;
                if (string.IsNullOrEmpty(name) || token is null)
                    continue;
                if (token[Mode] is null || token[Text] is null)
                    continue;
                CustomDialogueMode mode = new CustomDialogueMode(token[Mode].ToString());
                string text = token[Text].ToString();
                Map.Add(name, new CustomDialogue(mode, text));
            }
        }
        public CustomDialogueMode GetMode(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{this.GetType()}.{nameof(GetMode)} にnullまたは空文字列が渡されました。", nameof(name));
            if (!Map.ContainsKey(name))
                throw new KeyNotFoundException($"{this.GetType()}.{nameof(GetMode)} に渡された名前は存在しません。name={name}");
            return Map[name].Mode;
        }
        public string GetText(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{this.GetType()}.{nameof(GetText)} にnullまたは空文字列が渡されました。", nameof(name));
            if (!Map.ContainsKey(name))
                throw new KeyNotFoundException($"{this.GetType()}.{nameof(GetText)} に渡された名前は存在しません。name={name}");
            return Map[name].Text;
        }
        public class CustomDialogueMode
        {
            private const string Default = "default";
            private const string Custom = "custom";
            private const string None = "none";
            public string Mode { get; }
            public CustomDialogueMode(string mode)
            {
                if (string.IsNullOrEmpty(mode))
                    throw new ArgumentException($"{this.GetType()} のコンストラクタに null が渡されました。", nameof(mode));
                if (mode != Default || mode != Custom || mode != None)
                    throw new ArgumentException($"{this.GetType()} のコンストラクタに不正な文字列 {mode} が渡されました。", nameof(mode));
                Mode = mode;
            }
            public Boolean IsDefault()
            {
                return Mode == Default;
            }
            public Boolean IsCustom()
            {
                return Mode == Custom;
            }
            public Boolean IsNone()
            {
                return Mode == None;
            }
        }
        public class CustomDialogue
        {
            public CustomDialogueMode Mode { get; }
            public string Text { get;  }
            public CustomDialogue(CustomDialogueMode mode, string text)
            {
                Mode = mode;
                Text = text;
            }
        }
    }
}
