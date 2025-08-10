using MayhemFamiliar.QueueManager;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    class Verb
    {
        public const string Mulligan = "Mulligan";
        public const string GameStart = "GameStart";
        public const string GameOver = "GameOver";
        public const string NewTurnStarted = "NewTurnStarted";
        public const string ModifiedLife = "ModifiedLife";
        public static readonly string[] Speak = {
            ZoneTransferCategory.Discard,
            ZoneTransferCategory.PlayLand,
            ZoneTransferCategory.CastSpell,
            ZoneTransferCategory.Draw,
            ZoneTransferCategory.Sacrifice,
            Verb.Mulligan,
            Verb.GameStart,
            Verb.GameOver,
            Verb.NewTurnStarted,
            Verb.ModifiedLife
        };
    }
    internal class DialogueGenerator
    {
        public DialogueGenerator()
        {
        }
        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                Logger.Instance.Log($"{this.GetType().Name}: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (EventQueue.Queue.TryDequeue(out string eventString))
                    {
                        ProcessEvent(eventString);
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
                Logger.Instance.Log($"{this.GetType().Name}: エラー発生: {ex.Message}", LogLevel.Error);
            }
        }
        private void ProcessEvent(string eventString)
        {
            string subject = "";
            string verb = "";
            string objective = "";
            SplitEventString(eventString, ref subject, ref verb, ref objective);

            if (!Verb.Speak.Contains(verb))
            {
                return;
            }

            if (verb == Verb.NewTurnStarted)
            {
                if (int.TryParse(objective, out int turnNumber))
                {
                    if (turnNumber == 0)
                    {
                        // 0ターンの開始は無視（マリガンチェックなので）
                        Logger.Instance.Log($"{this.GetType().Name}: 0ターンの開始は無視", LogLevel.Debug);
                        return;
                    }
                }
            }

            string dialogue = "";
            switch (verb)
            {
                case Verb.Mulligan:
                    dialogue = "マリガンチェック。";
                    break;
                case Verb.GameStart:
                    dialogue = "対戦よろしくお願いします。";
                    break;
                case Verb.GameOver:
                    dialogue = "対戦ありがとうございました。";
                    break;
                case Verb.NewTurnStarted:
                    switch (subject)
                    {
                        case PlayerWho.You:
                            dialogue = "こちらのターン。";
                            break;
                        case PlayerWho.Opponent:
                            dialogue = "お相手のターン。";
                            break;
                        default:
                            dialogue = "誰かのターン。";
                            break;
                    }
                    break;
                case Verb.ModifiedLife:
                    string[] parts = objective.Split(' ');
                    if (parts.Length >= 2 &&
                        int.TryParse(parts[0], out int lifeDiff) &&
                        int.TryParse(parts[1], out int lifeTotal))
                    {
                        // ライフの変更を処理
                        switch (subject)
                        {
                            case PlayerWho.You:
                                if (lifeDiff < 0)
                                {
                                    dialogue = $"{Math.Abs(lifeDiff)}点受けて、ライフは{lifeTotal}。";
                                }
                                else
                                {
                                    dialogue = $"{Math.Abs(lifeDiff)}点回復して、ライフは{lifeTotal}。";
                                }
                                break;
                            case PlayerWho.Opponent:
                                if (lifeDiff < 0)
                                {
                                    dialogue = $"{Math.Abs(lifeDiff)}点与えて、お相手のライフは{lifeTotal}。";
                                }
                                else
                                {
                                    dialogue = $"{Math.Abs(lifeDiff)}点回復されて、お相手のライフは{lifeTotal}。";
                                }
                                break;
                            default:
                                dialogue = $"不明なプレイヤーのライフが{lifeDiff}点変更。";
                                break;
                        }
                    }
                    else
                    {
                        Logger.Instance.Log($"{this.GetType().Name}: ModifiedLifeの形式が不正: {objective}", LogLevel.Error);
                        return;
                    }
                    break;
            }
            if (!String.IsNullOrEmpty(dialogue))
            {
                Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
                DialogueQueue.Queue.Enqueue(dialogue);
                return;
            }

            switch (subject)
            {
                case PlayerWho.You:
                    break;
                case PlayerWho.Opponent:
                    dialogue = "お相手が";
                    break;
                default:
                    dialogue = "不明なプレイヤーが";
                    break;
            }
            if (!String.IsNullOrEmpty(objective))
            {
                if (!(subject == PlayerWho.Opponent && verb == ZoneTransferCategory.Draw))
                {
                    objective = RemoveObjectiveDelimiters(objective);
                    dialogue += objective + "を";
                }
            }
            switch (verb)
            {
                case ZoneTransferCategory.CastSpell:
                    dialogue += "キャスト。";
                    break;
                case ZoneTransferCategory.Discard:
                    dialogue += "ディスカード。";
                    break;
                case ZoneTransferCategory.Draw:
                    dialogue += "ドロー。";
                    break;
                case ZoneTransferCategory.PlayLand:
                    dialogue += "プレイ。";
                    break;
                case ZoneTransferCategory.Sacrifice:
                    dialogue += "生け贄に。";
                    break;
                default:
                    dialogue += "不明なアクション。";
                    break;
            }
            Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
            DialogueQueue.Queue.Enqueue(dialogue);
        }
        private void SplitEventString(string eventString, ref string subject, ref string verb, ref string objective)
        {
            // 入力がnullまたは空の場合は早期リターン
            if (string.IsNullOrEmpty(eventString))
            {
                return;
            }

            // 半角空白で分割、最大3つに制限
            string[] words = eventString.Split(new char[]{' '}, 3);

            // 配列の長さに応じて割り当て
            if (words.Length >= 1)
            {
                subject = words[0];
            }
            if (words.Length >= 2)
            {
                verb = words[1];
            }
            if (words.Length >= 3)
            {
                objective = words[2];
            }
        }
        private string ReplaceObjectiveDelimiters(string objective)
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
        private string RemoveObjectiveDelimiters(string objective)
        {
            // 目的語のデリミタを削除
            return objective.Replace("\"", "");
        }

    }
}
