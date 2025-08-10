using MayhemFamiliar.QueueManager;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class DialogueGenerator
    {
        private static readonly string[] IgnoreVerbs = {
            ZoneTransferCategory.Mill,
            ZoneTransferCategory.Nil,
            ZoneTransferCategory.Resolve,
            ZoneTransferCategory.Put,
        };
        private static readonly string[] ActiveVerbs = {
            ZoneTransferCategory.CastSpell,
            ZoneTransferCategory.Conjure,
            ZoneTransferCategory.Discard,
            ZoneTransferCategory.Draw,
            ZoneTransferCategory.Exile,
            ZoneTransferCategory.Mill,
            ZoneTransferCategory.PlayLand,
            ZoneTransferCategory.Put,
            ZoneTransferCategory.Sacrifice,
            ZoneTransferCategory.Resolve,
            ZoneTransferCategory.Return,
            AnnotationType.TokenCreated,
        };
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

            if (IgnoreVerbs.Contains(verb))
            {
                Logger.Instance.Log($"{this.GetType().Name}: 無視するアクション - {verb}", LogLevel.Debug);
                return;
            }

            if (verb == AnnotationType.NewTurnStarted)
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
                case GreMessageType.MulliganReq:
                    dialogue = "マリガンチェック。";
                    break;
                case GameStage.Start:
                    dialogue = "対戦よろしくお願いします。";
                    break;
                case GameStage.GameOver:
                    dialogue = "対戦ありがとうございました。";
                    break;
                case AnnotationType.NewTurnStarted:
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
                case AnnotationType.ModifiedLife:
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
                        Logger.Instance.Log($"{this.GetType().Name}: {verb}の形式が不正: {objective}", LogLevel.Error);
                        return;
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(dialogue))
            {
                Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
                DialogueQueue.Queue.Enqueue(dialogue);
                return;
            }

            if (ActiveVerbs.Contains(verb))
            {
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
                if (!string.IsNullOrEmpty(objective))
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
                    case ZoneTransferCategory.Conjure:
                        dialogue += "創出。";
                        break;
                    case ZoneTransferCategory.Discard:
                        dialogue += "ディスカード。";
                        break;
                    case ZoneTransferCategory.Draw:
                        dialogue += "ドロー。";
                        break;
                    case ZoneTransferCategory.Exile:
                        dialogue += "追放。";
                        break;
                    case ZoneTransferCategory.PlayLand:
                        dialogue += "プレイ。";
                        break;
                    case ZoneTransferCategory.Sacrifice:
                        dialogue += "生け贄に。";
                        break;
                    case ZoneTransferCategory.Return:
                        dialogue += "戦場に。";
                        break;
                    case ZoneTransferCategory.Put:
                    case ZoneTransferCategory.Resolve:
                        // 実況しない
                        break;
                    case AnnotationType.TokenCreated:
                        dialogue += "生成。";
                        break;
                    default:
                        dialogue += "不明なアクション。";
                        Logger.Instance.Log($"{this.GetType().Name}: 不明なアクション - {verb}", LogLevel.Debug);
                        break;
                }
            }
            else {
                switch (subject)
                {
                    case PlayerWho.You:
                        break;
                    case PlayerWho.Opponent:
                        dialogue = "お相手の";
                        break;
                    default:
                        dialogue = "不明なプレイヤーの";
                        break;
                }
                if (!string.IsNullOrEmpty(objective))
                {
                    objective = RemoveObjectiveDelimiters(objective);
                    dialogue += objective + "が";
                }
                switch (verb)
                {
                    case ZoneTransferCategory.SBA_Damage:
                    case ZoneTransferCategory.SBA_Deathtouch:
                    case ZoneTransferCategory.SBA_ZeroLoyalty:
                    case ZoneTransferCategory.SBA_ZeroToughness:
                        dialogue += "死亡。";
                        break;
                    case ZoneTransferCategory.SBA_UnattachedAura:
                        dialogue += "墓地に。";
                        break;
                    case ZoneTransferCategory.Destroy:
                        dialogue += "破壊。";
                        break;
                    case ZoneTransferCategory.Nil:
                        // 実況しない
                        break;
                    default:
                        dialogue += "不明なアクション。";
                        Logger.Instance.Log($"{this.GetType().Name}: 不明なアクション - {verb}", LogLevel.Debug);
                        break;
                }
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
