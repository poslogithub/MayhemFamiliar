using MayhemFamiliar.QueueManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
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
                if (key == EventDictKey.Name && ev.Dict[key] == Unknown.Name)
                {
                    text = text.Replace($"{{{key}}}", "カード");
                }
                text = text.Replace($"{{{key}}}", ev.Dict[key]);
            }
            Content = text;
        }
    }
    internal static class Mode
    {
        public const string Default = "default";
        public const string Custom = "custom";
        public const string None = "none";
    }
    internal static class Verb
    {
        public const string Mythic = "Mythic";
        public const string Rare = "Rare";
        public const string GainLife = "GainLife";
        public const string LoseLife = "LoseLife";
        public const string DrawKnownCard = "DrawKnownCard";
        public const string DrawUnknownCard = "DrawUnknownCard";
    }
    internal static class TextKey
    {
        public const string Event = "Event";
        public const string DestZoneType = "DestZoneType";
        public const string ZoneTransferCategory = "ZoneTransferCategory";
    }
    internal class DialogueGenerator
    {
        public CustomDialogues _limitedCustomDialogues;
        public CustomDialogues _YourCustomDialogues;
        public CustomDialogues _OpponentsCustomDialogues;
        public CustomDialogues _OpponentsThirdCustomDialogues;
        private const string LimitedCustomDialogsFilePath = "CustomDialogues_Limited.json";
        private const string YourCustomDialogsFilePath = "CustomDialogues_You.json";
        private const string OpponentsCustomDialogsFilePath = "CustomDialogues_Opponent.json";
        private const string OpponentsThirdCustomDialogsFilePath = "CustomDialogues_OpponentThird.json";

        public DialogueGenerator()
        {
            LoadAllCustomDialogues();
        }
        private CustomDialogues LoadCustomDialogues(string customDialogsFilePath)
        {
            if (string.IsNullOrEmpty(customDialogsFilePath)) return new CustomDialogues();
            if (!File.Exists(customDialogsFilePath)) return new CustomDialogues();

            try
            {
                return new CustomDialogues(customDialogsFilePath);
            }
            catch (FileNotFoundException ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: カスタム対話文のファイル {LimitedCustomDialogsFilePath} が存在しません。スキップします。");
                return new CustomDialogues();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: カスタム対話文の読み込み中にエラー発生: {ex.Message}", LogLevel.Error);
                throw new Exception($"カスタム対話文の読み込みに失敗しました。{customDialogsFilePath}", ex);
            }
        }
        private void LoadAllCustomDialogues()
        {
            _limitedCustomDialogues = LoadCustomDialogues(LimitedCustomDialogsFilePath);
            _YourCustomDialogues = LoadCustomDialogues(YourCustomDialogsFilePath);
            _OpponentsCustomDialogues = LoadCustomDialogues(OpponentsCustomDialogsFilePath);
            _OpponentsThirdCustomDialogues = LoadCustomDialogues(OpponentsThirdCustomDialogsFilePath);
        }
        private class DialogueTexts
        {
            public Dictionary<string, string> Map { get; }
            public DialogueTexts(Dictionary<string, string> dialogues)
            {
                Map = dialogues;
            }
        }
        private class DefaultPlayingDialogueTexts
        {
            public Dictionary<string, DialogueTexts> DialogueTextsForEvent { get; }
            public DefaultPlayingDialogueTexts(Dictionary<string, DialogueTexts> playingDialoguesForEvent)
            {
                DialogueTextsForEvent = playingDialoguesForEvent;
            }
        }
        private static readonly DialogueTexts LimitedDialogueTexts = 
            new DialogueTexts(
                new Dictionary<string, string>
                {
                    { DraftKey.Pick, "{name}をピック。"     },
                    { DraftKey.Pack, "{pack}の{pick}。"     },
                    { Verb.Mythic,   "神話レアは、{name}。" },
                    { Verb.Rare,     "レアは、{name}。"     },
                }
            );
        private static readonly DialogueTexts PlayingDefaultDialogueTextsForEvent = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { GameStage.Start,                "対戦よろしくお願いします。"       },
                { GreMessageType.MulliganReq,     "マリガンチェック。"               },
                { GameStage.GameOver,             "対戦ありがとうございました。"     },
                { AnnotationType.NewTurnStarted,  "こちらのターン。"                 },
                { AnnotationType.TokenCreated,    "{name}を生成。"                   },
                { Verb.GainLife,                  "{diff}点回復して、ライフは{to}。" },
                { Verb.LoseLife,                  "{diff}点受けて、ライフは{to}。"   },
                { ZoneTransferCategory.CastSpell, "{name}をキャスト。"               },
                { ZoneTransferCategory.Conjure,   "{name}を創出。"                   },
                { ZoneTransferCategory.Discard,   "{name}をディスカード。"           },
                { Verb.DrawKnownCard,             "{name}をドロー。"                 },
                { Verb.DrawUnknownCard,           "ドロー。"                         },
                { ZoneTransferCategory.Mill,      null }, // 実況しない
                { ZoneTransferCategory.Nil,       null }, // 実況しない
                { ZoneTransferCategory.PlayLand,  "{name}をプレイ。"                 },
                { ZoneTransferCategory.Resolve,   null }, // 実況しない
                { ZoneTransferCategory.Sacrifice, "{name}を生け贄に。"               },
                // { ZoneTransferCategory.Warp, "{name}がワープ。" },   // キーワード能力。ToZoneに任せる。
            }
        );
        private static readonly DialogueTexts PlayingDefaultDialogueTextsForDestZoneType = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneType.Revealed,    null }, // 実況しない
                { ZoneType.Suppressed,  null }, // 実況しない
                { ZoneType.Pending,     null }, // 実況しない
                { ZoneType.Command,     "{name}を統率領域に。"   },
                { ZoneType.Stack,       "{name}をキャスト。"     },
                { ZoneType.Battlefield, "{name}が戦場に。"       },
                { ZoneType.Exile,       "{name}が追放。"         },
                { ZoneType.Limbo,       null }, // 実況しない
                { ZoneType.Hand,        "{name}を手札に。"       },
                { ZoneType.Library,     "{name}をライブラリに。" },
                // { ZoneType.Graveyard, "{name}が墓地に。" },  // 墓地への移動は、カテゴリに応じて実況
                { ZoneType.Sideboard,   null }, // 実況しない、というか通常ありえない
            }
        );
        private static readonly DialogueTexts PlayingDefaultDialogueTextsForToGraveyard = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneTransferCategory.Destroy,           "{name}が破壊。"   },
                // { ZoneTransferCategory.SBA_UnattachedAura, "{name}が墓地に。" },  // その他扱い
                // { ZoneTransferCategory.SBA_ZeroLoyalty, "{name}が墓地に。" },     // その他扱い
                { ZoneTransferCategory.SBA_Damage,        "{name}が死亡。"   },
                { ZoneTransferCategory.SBA_Deathtouch,    "{name}が死亡。"   },
                { ZoneTransferCategory.SBA_ZeroToughness, "{name}が死亡。"   }, // クリーチャーの死亡はひとまとめにしたい
                { ZoneTransferCategory.Put,               "{name}を墓地に。" },
                { ZoneTransferCategory.Surveil,           "{name}を墓地に。" }, // 「墓地に置く」と「諜報」はひとまとめにしたい
                { "*",                                    "{name}が墓地に。" }, // その他のカテゴリ
            }
        );
        private static readonly DialogueTexts PlayingDefaultDialogueThirdTextsForEvent = new DialogueTexts(
    new Dictionary<string, string>()
    {
        { AnnotationType.NewTurnStarted,  "お相手のターン。"                    },
        { AnnotationType.TokenCreated,    "お相手が{name}を生成。"              },
        { Verb.GainLife,                  "{diff}点回復されて、ライフは{to}。"  },
        { Verb.LoseLife,                  "{diff}点与えて、ライフは{to}。"      },
        { ZoneTransferCategory.CastSpell, "お相手が{name}をキャスト。"          },
        { ZoneTransferCategory.Conjure,   "お相手が{name}を創出。"              },
        { ZoneTransferCategory.Discard,   "お相手が{name}をディスカード。"      },
        { Verb.DrawKnownCard,             "お相手が{name}をドロー。"            },
        { Verb.DrawUnknownCard,           "お相手がドロー。"                    },
        { ZoneTransferCategory.Mill,      null }, // 実況しない
        { ZoneTransferCategory.Nil,       null }, // 実況しない
        { ZoneTransferCategory.PlayLand,  "お相手が{name}をプレイ。"            },
        { ZoneTransferCategory.Resolve,   null }, // 実況しない
        { ZoneTransferCategory.Sacrifice, "お相手が{name}を生け贄に。"          },
        // { ZoneTransferCategory.Warp, "お相手の{name}がワープ。" },   // キーワード能力。ToZoneに任せる。
    }
);
        private static readonly DialogueTexts PlayingDefaultDialogueThirdTextsForDestZoneType = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneType.Revealed,    null }, // 実況しない
                { ZoneType.Suppressed,  null }, // 実況しない
                { ZoneType.Pending,     null }, // 実況しない
                { ZoneType.Command,     "お相手が{name}を統率領域に。"    },
                { ZoneType.Stack,       "お相手が{name}をキャスト。"      },
                { ZoneType.Battlefield, "お相手が{name}が戦場に。"        },
                { ZoneType.Exile,       "お相手が{name}が追放。"          },
                { ZoneType.Limbo,       null }, // 実況しない
                { ZoneType.Hand,        "お相手が{name}を手札に。"        },
                { ZoneType.Library,     "お相手が{name}をライブラリに。"  },
                // { ZoneType.Graveyard, "{name}が墓地に。" },  // 墓地への移動は、カテゴリに応じて実況
                { ZoneType.Sideboard,   null }, // 実況しない、というか通常ありえない
            }
        );
        private static readonly DialogueTexts PlayingDefaultDialogueThirdTextsForToGraveyard = new DialogueTexts(
            new Dictionary<string, string>()
            {
                { ZoneTransferCategory.Destroy,           "お相手の{name}が破壊。"    },
                // { ZoneTransferCategory.SBA_UnattachedAura, "{name}が墓地に。" },  // その他扱い
                // { ZoneTransferCategory.SBA_ZeroLoyalty, "{name}が墓地に。" },     // その他扱い
                { ZoneTransferCategory.SBA_Damage,        "お相手の{name}が死亡。"    },
                { ZoneTransferCategory.SBA_Deathtouch,    "お相手の{name}が死亡。"    },
                { ZoneTransferCategory.SBA_ZeroToughness, "お相手の{name}が死亡。"    }, // クリーチャーの死亡はひとまとめにしたい
                { ZoneTransferCategory.Put,               "お相手が{name}を墓地に。"  },
                { ZoneTransferCategory.Surveil,           "お相手が{name}を墓地に。"  }, // 「墓地に置く」と「諜報」はひとまとめにしたい
                { "*",                                    "お相手の{name}が墓地に。"  }, // その他のカテゴリ
            }
        );
        private static readonly DefaultPlayingDialogueTexts YourTexts =
            new DefaultPlayingDialogueTexts(
                new Dictionary<string, DialogueTexts>()
                {
                    { TextKey.Event, PlayingDefaultDialogueTextsForEvent },
                    { TextKey.DestZoneType, PlayingDefaultDialogueTextsForDestZoneType },
                    { TextKey.ZoneTransferCategory, PlayingDefaultDialogueTextsForToGraveyard }
                }
            );
        private static readonly DefaultPlayingDialogueTexts OpponentsTexts = YourTexts; // 対戦相手のデフォルト対話文は自分と同じ
        private static readonly DefaultPlayingDialogueTexts OpponentsThirdTexts = new DefaultPlayingDialogueTexts(
                new Dictionary<string, DialogueTexts>()
                {
                    { TextKey.Event, PlayingDefaultDialogueThirdTextsForEvent },
                    { TextKey.DestZoneType, PlayingDefaultDialogueThirdTextsForDestZoneType },
                    { TextKey.ZoneTransferCategory, PlayingDefaultDialogueThirdTextsForToGraveyard }
                }
            );

        private string GetLimitedText(Event ev, CustomDialogues limitedCustomDialogues)
        {
            if (!limitedCustomDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                return LimitedDialogueTexts.Map[ev.Verb];
            if (limitedCustomDialogues.GetMode(ev.Verb).IsNone())
                return "";
            if (limitedCustomDialogues.GetMode(ev.Verb).IsCustom())
                return limitedCustomDialogues.GetText(ev.Verb);
            return LimitedDialogueTexts.Map[ev.Verb];
        }
        private string GetPlayingText(Event ev, DefaultPlayingDialogueTexts defaultPlayingDialogues, CustomDialogues customDialogues)
        {
            if (defaultPlayingDialogues.DialogueTextsForEvent.ContainsKey(TextKey.Event) && defaultPlayingDialogues.DialogueTextsForEvent[TextKey.Event].Map.ContainsKey(ev.Verb))
            {
                if (!customDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                    return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.Event].Map[ev.Verb];
                if (customDialogues.GetMode(ev.Verb).IsNone())
                    return "";
                if (customDialogues.GetMode(ev.Verb).IsCustom())
                    return customDialogues.GetText(ev.Verb);
                return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.Event].Map[ev.Verb];
            }
            if (defaultPlayingDialogues.DialogueTextsForEvent.ContainsKey(TextKey.DestZoneType) && defaultPlayingDialogues.DialogueTextsForEvent[TextKey.DestZoneType].Map.ContainsKey(ev.Dict[EventDictKey.To]))
            {
                if (!customDialogues.Map.ContainsKey(ev.Dict[EventDictKey.To]))   // そもそも定義されていなければデフォルトを返す
                    return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.DestZoneType].Map[ev.Dict[EventDictKey.To]];
                if (customDialogues.GetMode(ev.Dict[EventDictKey.To]).IsNone())
                    return "";
                if (customDialogues.GetMode(ev.Dict[EventDictKey.To]).IsCustom())
                    return customDialogues.GetText(ev.Dict[EventDictKey.To]);
                return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.DestZoneType].Map[ev.Dict[EventDictKey.To]];
            }
            if (defaultPlayingDialogues.DialogueTextsForEvent.ContainsKey(TextKey.ZoneTransferCategory) && ev.Dict[EventDictKey.To] == ZoneType.Graveyard)
            {
                if (!customDialogues.Map.ContainsKey(ev.Verb))   // そもそも定義されていなければデフォルトを返す
                {
                    if (defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map.ContainsKey(ev.Verb))
                        return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map[ev.Verb];
                    if (defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map.ContainsKey("*"))
                        return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map["*"];
                    return "";
                }
                if (customDialogues.GetMode(ev.Verb).IsNone())
                    return "";
                if (customDialogues.GetMode(ev.Verb).IsCustom())
                    return customDialogues.GetText(ev.Verb);
                if (defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map.ContainsKey(ev.Verb))
                    return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map[ev.Verb];
                if (defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map.ContainsKey("*"))
                    return defaultPlayingDialogues.DialogueTextsForEvent[TextKey.ZoneTransferCategory].Map["*"];
            }
            return "";
        }
        private string GetText(Event ev)
        {
            switch (ev.PlayerWho)
            {
                case PlayerWho.You:
                    if (LimitedDialogueTexts.Map.ContainsKey(ev.Verb))
                        return GetLimitedText(ev, _limitedCustomDialogues);
                    else
                        return GetPlayingText(ev, YourTexts, _YourCustomDialogues);
                case PlayerWho.Opponent:
                    if (Program._config.SpeakerSettings.SpeakModes[PlayerWho.Opponent] == Config.Speaker.SpeakModeThird)
                        return GetPlayingText(ev, OpponentsThirdTexts, _OpponentsThirdCustomDialogues);
                    else
                        return GetPlayingText(ev, OpponentsTexts, _OpponentsCustomDialogues);
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
            string text = GetText(ev);
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
