using MayhemFamiliar.Logging;
using MayhemFamiliar.Queues;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class Dialogue
    {
        public string PlayerWho { get; }
        public string Content { get; }
        public Dialogue(Event ev, string text)
        {
            PlayerWho = ev.PlayerWho;
            foreach (string key in ev.Dict.Keys)
            {
                if (key == EventDictKey.Name && ev.Dict[key] == Unknown.Name)   // 不明なカード名は「カード」に置換
                {
                    text = text.Replace($"{{{key}}}", "カード");
                }
                text = text.Replace($"{{{key}}}", ev.Dict[key]);
            }
            Content = text;
        }
    }

    internal static class Verb
    {
        public const string Mythic = "Mythic";
        public const string Rare = "Rare";
        public const string GainLife = "GainLife";
        public const string LoseLife = "LoseLife";
        public const string DrawKnownCard = "DrawKnownCard";
        public const string DrawUnknownCard = "DrawUnknownCard";
        public const string Die = "Die";
        public const string PutGraveyard = "PutGraveyard";
    }
    internal static class TextKey
    {
        public const string Action = "Action";
        public const string DestZoneType = "DestZoneType";
        public const string ZoneTransferCategory = "ZoneTransferCategory";
    }
    internal class DialogueGenerator
    {
        public CustomDialogueTexts _limitedCustomDialogueTexts;
        public CustomDialogueTexts _YourCustomDialogueTexts;
        public CustomDialogueTexts _OpponentsCustomDialogueTexts;
        public CustomDialogueTexts _OpponentsThirdCustomDialogueTexts;
        private const string LimitedCustomDialogueTextsFilePath = "CustomDialogueTexts_Limited.json";
        private const string YourCustomDialogueTextsFilePath = "CustomDialogueTexts_You.json";
        private const string OpponentsCustomDialogueTextsFilePath = "CustomDialogueTexts_Opponent.json";
        private const string OpponentsThirdCustomDialogueTextsFilePath = "CustomDialogueTexts_OpponentThird.json";

        public DialogueGenerator()
        {
            LoadAllCustomDialogueTexts();
        }
        private CustomDialogueTexts LoadCustomDialogueTexts(string customDialogueTextsFilePath)
        {
            if (string.IsNullOrEmpty(customDialogueTextsFilePath)) return new CustomDialogueTexts();
            if (!File.Exists(customDialogueTextsFilePath)) return new CustomDialogueTexts();

            try
            {
                return new CustomDialogueTexts(customDialogueTextsFilePath);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: カスタムテキストファイルの読み込み中にエラー発生: {ex.Message}", LogLevel.Error);
                throw new Exception($"カスタムテキストファイルの読み込みに失敗しました。{customDialogueTextsFilePath}", ex);
            }
        }
        private void LoadAllCustomDialogueTexts()
        {
            _limitedCustomDialogueTexts = LoadCustomDialogueTexts(LimitedCustomDialogueTextsFilePath);
            _YourCustomDialogueTexts = LoadCustomDialogueTexts(YourCustomDialogueTextsFilePath);
            _OpponentsCustomDialogueTexts = LoadCustomDialogueTexts(OpponentsCustomDialogueTextsFilePath);
            _OpponentsThirdCustomDialogueTexts = LoadCustomDialogueTexts(OpponentsThirdCustomDialogueTextsFilePath);
        }
        internal class CustomDialogueTexts
        {
            private const string Mode = "mode";
            private const string Text = "text";
            public Dictionary<string, CustomDialogue> Map { get; } = new Dictionary<string, CustomDialogue>();
            public CustomDialogueTexts() { }
            public CustomDialogueTexts(string customDialoguesFilePath)
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
                private const string Off = "off";
                public string Mode { get; }
                public CustomDialogueMode(string mode)
                {
                    if (string.IsNullOrEmpty(mode))
                        throw new ArgumentException($"{this.GetType()} のコンストラクタに null が渡されました。", nameof(mode));
                    string lowerMode = mode.ToLower();
                    if (lowerMode != Default && lowerMode != Custom && lowerMode != Off)
                        throw new ArgumentException($"{this.GetType()} のコンストラクタに不正な文字列 {mode} が渡されました。", nameof(mode));
                    Mode = lowerMode;
                }
                public Boolean IsDefault()
                {
                    return string.Equals(Mode, Default, StringComparison.OrdinalIgnoreCase);
                }
                public Boolean IsCustom()
                {
                    return string.Equals(Mode, Custom, StringComparison.OrdinalIgnoreCase);
                }
                public Boolean IsOff()
                {
                    return string.Equals(Mode, Off, StringComparison.OrdinalIgnoreCase);
                }
            }
            public class CustomDialogue
            {
                public CustomDialogueMode Mode { get; }
                public string Text { get; }
                public CustomDialogue(CustomDialogueMode mode, string text)
                {
                    Mode = mode;
                    Text = text;
                }
            }
        }
        private class DialogueTexts
        {
            public Dictionary<string, string> Map { get; }
            public DialogueTexts(Dictionary<string, string> texts)
            {
                Map = texts;
            }
        }
        private class PlayingDialogueTextsForEvent
        {
            public Dictionary<string, DialogueTexts> Map { get; }
            public PlayingDialogueTextsForEvent(Dictionary<string, DialogueTexts> playingDialoguesForEvent)
            {
                Map = playingDialoguesForEvent;
            }
        }
        private static readonly DialogueTexts LimitedDialogueTexts = new DialogueTexts(
            new Dictionary<string, string>
            {
                { DraftKey.Pick, "{name}をピック。"     },
                { DraftKey.Pack, "{pack}の{pick}。"     },
                { Verb.Mythic,   "神話レアは、{name}。" },
                { Verb.Rare,     "レアは、{name}。"     },
            }
        );
        private static readonly DialogueTexts DefaultPlayingDialogueTextsForVerb = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { GameStage.Start,                "対戦よろしくおねがいします。"     },
                { GreMessageType.MulliganReq,     "マリガンチェック。"               },
                { GameStage.GameOver,             "対戦ありがとうございました。"     },
                { AnnotationType.NewTurnStarted,  "こちらのターン。"                 },
                { Verb.DrawKnownCard,             "{name}をドロー。"                 },
                { Verb.DrawUnknownCard,           "ドロー。"                         },
                { ZoneTransferCategory.Discard,   "{name}をディスカード。"           },
                { ZoneTransferCategory.PlayLand,  "{name}をプレイ。"                 },
                { ZoneTransferCategory.CastSpell, "{name}をキャスト。"               },
                { Verb.GainLife,                  "{diff}点回復して、ライフは{to}。" },
                { Verb.LoseLife,                  "{diff}点受けて、ライフは{to}。"   },
                { ZoneTransferCategory.Sacrifice, "{name}を生け贄に。"               },
                { AnnotationType.TokenCreated,    "{name}を生成。"                   },
                { ZoneTransferCategory.Conjure,   "{name}を創出。"                   },
                { ZoneTransferCategory.Mill,      "" }, // 実況しない
                { ZoneTransferCategory.Resolve,   "" }, // 実況しない
                { ZoneTransferCategory.Nil,       "" }, // 実況しない
            }
        );
        private static readonly DialogueTexts DefaultPlayingDialogueTextsForDestZoneType = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneType.Revealed,    "" }, // 実況しない
                { ZoneType.Suppressed,  "" }, // 実況しない
                { ZoneType.Pending,     "" }, // 実況しない
                { ZoneType.Command,     "{name}を統率領域に。"   },
                { ZoneType.Stack,       "" }, // 実況しない、というか通常ありえない
                { ZoneType.Battlefield, "{name}が戦場に。"       },
                { ZoneType.Exile,       "{name}が追放。"         },
                { ZoneType.Limbo,       "" }, // 実況しない
                { ZoneType.Hand,        "{name}が手札に。"       },
                { ZoneType.Library,     "{name}がライブラリに。" },
                // { ZoneType.Graveyard, "{name}が墓地に。" },  // 墓地への移動は、カテゴリに応じて実況
                { ZoneType.Sideboard,   "" }, // 実況しない、というか通常ありえない
            }
        );
        private static readonly DialogueTexts DefaultPlayingDialogueTextsForToGraveyard = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneTransferCategory.Destroy, "{name}が破壊。"   },
                { Verb.Die,                     "{name}が死亡。"   },
                { "*",                          "{name}が墓地に。" }, // その他のカテゴリ
            }
        );
        private static readonly DialogueTexts ThirdDefaultPlayingDialogueTextsForVerb = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { AnnotationType.NewTurnStarted,  "お相手のターン。"                    },
                { Verb.DrawUnknownCard,           "お相手がドロー。"                    },
                { Verb.DrawKnownCard,             "お相手が{name}をドロー。"            },
                { ZoneTransferCategory.Discard,   "お相手が{name}をディスカード。"      },
                { ZoneTransferCategory.PlayLand,  "お相手が{name}をプレイ。"            },
                { ZoneTransferCategory.CastSpell, "お相手が{name}をキャスト。"          },
                { Verb.GainLife,                  "{diff}点回復されて、ライフは{to}。"  },
                { Verb.LoseLife,                  "{diff}点与えて、ライフは{to}。"      },
                { ZoneTransferCategory.Sacrifice, "お相手が{name}を生け贄に。"          },
                { AnnotationType.TokenCreated,    "お相手が{name}を生成。"              },
                { ZoneTransferCategory.Conjure,   "お相手が{name}を創出。"              },
                { ZoneTransferCategory.Mill,      "" }, // 実況しない
                { ZoneTransferCategory.Resolve,   "" }, // 実況しない
                { ZoneTransferCategory.Nil,       "" }, // 実況しない
            }
        );
        private static readonly DialogueTexts ThirdDefaultPlayingDialogueTextsForDestZoneType = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneType.Revealed,    "" }, // 実況しない
                { ZoneType.Suppressed,  "" }, // 実況しない
                { ZoneType.Pending,     "" }, // 実況しない
                { ZoneType.Command,     "お相手が{name}を統率領域に。"    },
                { ZoneType.Stack,       "" }, // 実況しない、というか通常ありえない
                { ZoneType.Battlefield, "お相手の{name}が戦場に。"        },
                { ZoneType.Exile,       "お相手の{name}が追放。"          },
                { ZoneType.Limbo,       "" }, // 実況しない
                { ZoneType.Hand,        "お相手の{name}が手札に。"        },
                { ZoneType.Library,     "お相手の{name}がライブラリに。"  },
                // { ZoneType.Graveyard, "{name}が墓地に。" },  // 墓地への移動は、カテゴリに応じて実況
                { ZoneType.Sideboard,   "" }, // 実況しない、というか通常ありえない
            }
        );
        private static readonly DialogueTexts ThirdDefaultPlayingDialogueTextsForToGraveyard = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneTransferCategory.Destroy, "お相手の{name}が破壊。"    },
                { Verb.Die,                     "お相手の{name}が死亡。"    }, 
                { "*",                          "お相手の{name}が墓地に。"  }, // その他のカテゴリ
            }
        );
        private static readonly PlayingDialogueTextsForEvent YourDefaultPlayingDialogueTextsForEvent =
            new PlayingDialogueTextsForEvent(
                new Dictionary<string, DialogueTexts>()
                {
                    { TextKey.Action, DefaultPlayingDialogueTextsForVerb },
                    { TextKey.DestZoneType, DefaultPlayingDialogueTextsForDestZoneType },
                    { TextKey.ZoneTransferCategory, DefaultPlayingDialogueTextsForToGraveyard }
                }
            );
        private static readonly PlayingDialogueTextsForEvent OpponentsDefaultPlayingDialogueTextsForEvent = YourDefaultPlayingDialogueTextsForEvent; // 対戦相手のデフォルト対話文は自分と同じ
        private static readonly PlayingDialogueTextsForEvent OpponentsThirdDefaultPlayingDialogueTextsForEvent = new PlayingDialogueTextsForEvent(
                new Dictionary<string, DialogueTexts>()
                {
                    { TextKey.Action, ThirdDefaultPlayingDialogueTextsForVerb },
                    { TextKey.DestZoneType, ThirdDefaultPlayingDialogueTextsForDestZoneType },
                    { TextKey.ZoneTransferCategory, ThirdDefaultPlayingDialogueTextsForToGraveyard }
                }
            );

        private string GetLimitedDialogueText(Event ev, CustomDialogueTexts limitedCustomDialogues)
        {
            if (!limitedCustomDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                return LimitedDialogueTexts.Map[ev.Verb];
            if (limitedCustomDialogues.GetMode(ev.Verb).IsOff())
                return "";
            if (limitedCustomDialogues.GetMode(ev.Verb).IsCustom())
                return limitedCustomDialogues.GetText(ev.Verb);
            return LimitedDialogueTexts.Map[ev.Verb];
        }
        private string GetPlayingDialogueText(Event ev, PlayingDialogueTextsForEvent defaultPlayingDialogues, CustomDialogueTexts customDialogues)
        {
            if (defaultPlayingDialogues.Map.ContainsKey(TextKey.Action) && defaultPlayingDialogues.Map[TextKey.Action].Map.ContainsKey(ev.Verb))
            {
                if (!customDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                    return defaultPlayingDialogues.Map[TextKey.Action].Map[ev.Verb];
                if (customDialogues.GetMode(ev.Verb).IsOff())
                    return "";
                if (customDialogues.GetMode(ev.Verb).IsCustom())
                    return customDialogues.GetText(ev.Verb);
                return defaultPlayingDialogues.Map[TextKey.Action].Map[ev.Verb];
            }
            if (defaultPlayingDialogues.Map.ContainsKey(TextKey.DestZoneType) && defaultPlayingDialogues.Map[TextKey.DestZoneType].Map.ContainsKey(ev.Dict[EventDictKey.To]))
            {
                if (!customDialogues.Map.ContainsKey(ev.Dict[EventDictKey.To]))   // そもそも定義されていなければデフォルトを返す
                    return defaultPlayingDialogues.Map[TextKey.DestZoneType].Map[ev.Dict[EventDictKey.To]];
                if (customDialogues.GetMode(ev.Dict[EventDictKey.To]).IsOff())
                    return "";
                if (customDialogues.GetMode(ev.Dict[EventDictKey.To]).IsCustom())
                    return customDialogues.GetText(ev.Dict[EventDictKey.To]);
                return defaultPlayingDialogues.Map[TextKey.DestZoneType].Map[ev.Dict[EventDictKey.To]];
            }
            if (defaultPlayingDialogues.Map.ContainsKey(TextKey.ZoneTransferCategory) && ev.Dict[EventDictKey.To] == ZoneType.Graveyard)
            {
                if (!customDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                {
                    if (defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map.ContainsKey(ev.Verb))
                        return defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map[ev.Verb];
                    if (defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map.ContainsKey("*"))
                        return defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map["*"];
                    return "";
                }
                if (customDialogues.GetMode(ev.Verb).IsOff())
                    return "";
                if (customDialogues.GetMode(ev.Verb).IsCustom())
                    return customDialogues.GetText(ev.Verb);
                if (defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map.ContainsKey(ev.Verb))
                    return defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map[ev.Verb];
                if (defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map.ContainsKey("*"))
                    return defaultPlayingDialogues.Map[TextKey.ZoneTransferCategory].Map["*"];
            }
            return "";
        }
        private string GetDialogueTextForEvent(Event ev)
        {
            switch (ev.PlayerWho)
            {
                case PlayerWho.You:
                    if (LimitedDialogueTexts.Map.ContainsKey(ev.Verb))
                        return GetLimitedDialogueText(ev, _limitedCustomDialogueTexts);
                    else
                        return GetPlayingDialogueText(ev, YourDefaultPlayingDialogueTextsForEvent, _YourCustomDialogueTexts);
                case PlayerWho.Opponent:
                    if (Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] == Config.Speaker.SpeakModeThird)
                        return GetPlayingDialogueText(ev, OpponentsThirdDefaultPlayingDialogueTextsForEvent, _OpponentsThirdCustomDialogueTexts);
                    else
                        return GetPlayingDialogueText(ev, OpponentsDefaultPlayingDialogueTextsForEvent, _OpponentsCustomDialogueTexts);
            }
            return "";
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                Logger.Instance.Log($"{this.GetType().Name}: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (EventQueue.Queue.TryDequeue(out Event ev))
                    {
                        try
                        {
                            string subject = ev.PlayerWho;
                            if (string.IsNullOrEmpty(subject))
                            {
                                Logger.Instance.Log($"{this.GetType().Name}: イベントの主体が不明: {ev}", LogLevel.Error);
                            }
                            else
                            {
                                ProcessEvent(ev);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Log($"{this.GetType().Name}: イベントの処理中にエラー発生: {ex.Message} - {ev}", LogLevel.Error);
                        }
                        await Task.Delay(10, cancellationToken);
                    }
                    else await Task.Delay(100, cancellationToken);
                }
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (OperationCanceledException)
            {
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: エラー発生: {ex.Message}", LogLevel.Error);
            }
        }
        private void ProcessEvent(Event ev)
        {
            string text = GetDialogueTextForEvent(ev);
            if (string.IsNullOrEmpty(text)) return;
            Dialogue dialogue = new Dialogue(ev, text);
            if (string.IsNullOrEmpty(dialogue.Content)) return;
            DialogueQueue.Queue.Enqueue(dialogue);
        }
        private string[] SplitEventString(string eventString)
        {
            List<string> result = new List<string>();

            // 入力がnullまたは空の場合は早期リターン
            if (string.IsNullOrEmpty(eventString)) return result.ToArray();
            
            int i = 0;
            while (i < eventString.Length)
            {
                // 空白をスキップ
                while (i < eventString.Length && char.IsWhiteSpace(eventString[i]))
                    i++;

                if (i >= eventString.Length)
                    break;

                if (eventString[i] == '"')
                {
                    // クォートされた文字列を処理
                    i++;
                    int start = i;
                    while (i < eventString.Length && eventString[i] != '"')
                        i++;
                    if (i < eventString.Length)
                    {
                        result.Add($"\"{eventString.Substring(start, i - start)}\"");
                        i++;
                    }
                }
                else
                {
                    // クォートされていないトークンを処理
                    int start = i;
                    while (i < eventString.Length && !char.IsWhiteSpace(eventString[i]) && eventString[i] != '"')
                        i++;
                    result.Add(eventString.Substring(start, i - start));
                }
            }

            return result.ToArray();
        }

        /* private string ReplaceObjectiveDelimiters(string objective)
        {
            if (CultureInfo.CurrentUICulture.Name == "ja-JP")
            {
                // 目的語のデリミタを置換
                int first = objective.IndexOf('"');
                if (first == -1) return objective;
                int second = objective.IndexOf('"', first + 1);
                if (second == -1) return objective;

                var chars = objective.ToCharArray();
                chars[first] = '《';
                chars[second] = '》';
                return new string(chars);
                // 目的語のデリミタを削除
                objective = objective.Replace("\"", "");
            }
            return objective;
        }
        */
    }
}
