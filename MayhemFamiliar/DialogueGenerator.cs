using MayhemFamiliar.QueueManager;
using System;
using System.Collections.Generic;
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
            // ZoneTransferCategory.Put,
            ZoneTransferCategory.Resolve,
            ZoneTransferCategory.Surveil,
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
            ZoneTransferCategory.Warp,
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
            string[] tokens = SplitEventString(eventString);

            // トークン数が2未満（verbが無い）ならエラー
            if (tokens.Length < 2)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {verb} イベントのトークン数が2未満: {eventString}", LogLevel.Error);
                return;
            }
            subject = tokens[0];
            verb = tokens[1];

            // 特定のZoneTransferCategoryは無視する
            if (IgnoreVerbs.Contains(verb))
            {
                Logger.Instance.Log($"{this.GetType().Name}: 無視するアクション - {verb}", LogLevel.Debug);
                return;
            }

            // トークン数 = 2
            string dialogue = "";
            switch (verb)
            {
                case GameStage.Start:
                    dialogue = "対戦よろしくお願いします。";
                    break;
                case GreMessageType.MulliganReq:
                    dialogue = "マリガンチェック。";
                    break;
                case GameStage.GameOver:
                    dialogue = "対戦ありがとうございました。";
                    break;
            }
            if (!string.IsNullOrEmpty(dialogue))
            {
                Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
                DialogueQueue.Queue.Enqueue(dialogue);
                return;
            }

            // トークン数 = 3
            if (tokens.Length < 3)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {verb} イベントのトークン数が3未満: {eventString}", LogLevel.Error);
                return;
            }
            switch (verb)
            {
                case AnnotationType.NewTurnStarted:
                    int turnNumber;
                    try
                    {
                        turnNumber = int.Parse(tokens[2]);
                    }
                    catch (FormatException)
                    {
                        Logger.Instance.Log($"{this.GetType().Name}: ターン数の形式が不正: {tokens[2]}", LogLevel.Error);
                        return;
                    }
                    if (turnNumber == 0)
                    {
                        // 0ターンの開始は無視（マリガンチェックなので）
                        Logger.Instance.Log($"{this.GetType().Name}: 0ターンの開始は無視", LogLevel.Debug);
                        return;
                    }
                    switch (subject)
                    {
                        case PlayerWho.You:
                            dialogue = "こちらのターン。";
                            break;
                        case PlayerWho.Opponent:
                            dialogue = "お相手のターン。";
                            break;
                        default:
                            Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤーのターン", LogLevel.Debug);
                            break;
                    }
                    break;
                case AnnotationType.TokenCreated:
                    string tokenName = RemoveObjectiveDelimiters(tokens[2]);
                    switch (subject)
                    {
                        case PlayerWho.You:
                            break;
                        case PlayerWho.Opponent:
                            dialogue = "お相手が";
                            break;
                        default:
                            dialogue = "不明なプレイヤーが";
                            Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤーがトークン生成: {eventString}", LogLevel.Debug);
                            break;
                    }
                    dialogue += $"{tokenName}を生成。";
                    break;
            }
            if (!string.IsNullOrEmpty(dialogue))
            {
                Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
                DialogueQueue.Queue.Enqueue(dialogue);
                return;
            }

            // トークン数 = 4（ライフ増減）
            if (tokens.Length < 4)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {verb} イベントのトークン数が4未満: {eventString}", LogLevel.Error);
                return;
            }
            switch (verb)
            {
                case AnnotationType.ModifiedLife:
                    int lifeDiff, lifeTotal;
                    try
                    {
                        lifeDiff = int.Parse(tokens[2]);
                        lifeTotal = int.Parse(tokens[3]);
                    }
                    catch (FormatException)
                    {
                        Logger.Instance.Log($"{this.GetType().Name}: {verb} ライフの形式が不正: {eventString}", LogLevel.Error);
                        return;
                    }
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
                            Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤーのライフ変動: {eventString}", LogLevel.Debug);
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

            // トークン数 = 5（ゾーン間移動）
            if (tokens.Length < 5)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {verb} イベントのトークン数が5未満: {eventString}", LogLevel.Error);
                return;
            }
            string objectName = RemoveObjectiveDelimiters(tokens[2]);
            int zoneSrcId, zoneDestId;
            try
            {
                zoneSrcId = int.Parse(tokens[3]);
                zoneDestId = int.Parse(tokens[4]);
            }
            catch (FormatException)
            {
                Logger.Instance.Log($"{this.GetType().Name}: {verb} ゾーンIDの形式が不正: {eventString}", LogLevel.Error);
                return;
            }
            switch (subject)
            {
                case PlayerWho.You:
                    break;
                case PlayerWho.Opponent:
                    dialogue = "お相手";
                    break;
                default:
                    dialogue = "不明なプレイヤー";
                    Logger.Instance.Log($"{this.GetType().Name}: 不明なプレイヤーのアクション: {eventString}", LogLevel.Debug);
                    break;
            }
            switch (verb)
            {
                // 特定のverbの場合は、移動先ゾーンに依らず決め打ちで実況
                case ZoneTransferCategory.CastSpell:
                    dialogue += subject == PlayerWho.You ? "" : "が";
                    dialogue += $"{objectName}をキャスト。";
                    break;
                case ZoneTransferCategory.Conjure:
                    dialogue += subject == PlayerWho.You ? "" : "が";
                    dialogue += $"{objectName}を創出。";
                    break;
                case ZoneTransferCategory.Discard:
                    dialogue += subject == PlayerWho.You ? "" : "が";
                    dialogue += $"{objectName}をディスカード。";
                    break;
                case ZoneTransferCategory.Draw:
                    if (subject != PlayerWho.You && objectName == Unknown.Name)
                    {
                        dialogue += $"がドロー。";
                    }
                    else
                    {
                        dialogue += subject == PlayerWho.You ? "" : "が";
                        dialogue += $"{objectName}をドロー。";
                    }
                    break;
                case ZoneTransferCategory.PlayLand:
                    dialogue += subject == PlayerWho.You ? "" : "が";
                    dialogue += $"{objectName}をプレイ。";
                    break;
                case ZoneTransferCategory.Sacrifice:
                    dialogue += subject == PlayerWho.You ? "" : "が";
                    dialogue += $"{objectName}を生け贄に。";
                    break;
                // 破壊や死亡は置換される可能性があるため、ここでは扱わない。
                // ここからはキーワード能力
                case ZoneTransferCategory.Warp:
                    dialogue += subject == PlayerWho.You ? "" : "の";
                    dialogue += $"{objectName}がワープ。";
                    break;
                default:
                    // 特定verb以外は移動先ゾーンに応じて実況
                    switch (zoneDestId)
                    {
                        case ZoneId.Command:
                            dialogue += subject == PlayerWho.You ? "" : "が";
                            dialogue += $"{objectName}を統率領域に。";
                            break;
                        case ZoneId.Stack:
                            dialogue += subject == PlayerWho.You ? "" : "が";
                            dialogue += $"{objectName}をキャスト。";
                            break;
                        case ZoneId.Battlefield:
                            dialogue += subject == PlayerWho.You ? "" : "の";
                            dialogue += $"{objectName}が戦場に。";
                            break;
                        case ZoneId.Exile:
                            dialogue += subject == PlayerWho.You ? "" : "の";
                            dialogue += $"{objectName}が追放。";
                            // 発見や続唱による追放の実況はしたくない
                            break;
                        case ZoneId.Hand1:
                        case ZoneId.Hand2:
                            dialogue += subject == PlayerWho.You ? "" : "が";
                            if (subject != PlayerWho.You && objectName == Unknown.Name)
                            {
                                objectName = "カード";
                            }
                            dialogue += $"{objectName}を手札に。";
                            break;
                        case ZoneId.Library1:
                        case ZoneId.Library2:
                            dialogue += subject == PlayerWho.You ? "" : "が";
                            if (subject != PlayerWho.You && objectName == Unknown.Name)
                            {
                                objectName = "カード";
                            }
                            dialogue += $"{objectName}をライブラリに。";
                            // 発見や続唱による追放領域から戻した場合の実況はしたくない
                            break;
                        case ZoneId.Graveyard1:
                        case ZoneId.Graveyard2:
                            // 墓地への移動は、カテゴリに応じて実況
                            switch (verb)
                            {
                                case ZoneTransferCategory.Destroy:
                                    dialogue += subject == PlayerWho.You ? "" : "の";
                                    dialogue += $"{objectName}が破壊。";
                                    break;
                                case ZoneTransferCategory.SBA_UnattachedAura:
                                    dialogue += subject == PlayerWho.You ? "" : "の";
                                    dialogue += $"{objectName}が墓地に。";
                                    break;
                                case ZoneTransferCategory.SBA_Damage:
                                case ZoneTransferCategory.SBA_Deathtouch:
                                case ZoneTransferCategory.SBA_ZeroLoyalty:
                                case ZoneTransferCategory.SBA_ZeroToughness:
                                    dialogue += subject == PlayerWho.You ? "" : "の";
                                    dialogue += $"{objectName}が死亡。";
                                    break;
                                case ZoneTransferCategory.Put:
                                case ZoneTransferCategory.Surveil:
                                    dialogue += subject == PlayerWho.You ? "" : "が";
                                    dialogue += $"{objectName}を墓地に。";
                                    break;
                                case ZoneTransferCategory.Mill:
                                case ZoneTransferCategory.Nil:
                                case ZoneTransferCategory.Resolve:
                                    // 実況しない
                                    Logger.Instance.Log($"{this.GetType().Name}: 実況しないアクション: {eventString}", LogLevel.Debug);
                                    break;
                                default:
                                    dialogue += subject == PlayerWho.You ? "" : "の";
                                    dialogue += $"{objectName}が墓地に。";
                                    break;
                            }
                            break;
                        case ZoneId.Sideboard1:
                        case ZoneId.Sideboard2:
                            // 実況しない、というか通常ありえない
                            Logger.Instance.Log($"{this.GetType().Name}: 実況しないアクション: {eventString}", LogLevel.Debug);
                            break;
                        default:
                            // 不明なゾーンID
                            Logger.Instance.Log($"{this.GetType().Name}: 不明なゾーンID: {zoneDestId} - {eventString}", LogLevel.Error);
                            return;
                    }
                    break;
            }
            Logger.Instance.Log($"{this.GetType().Name}: {dialogue}", LogLevel.Debug);
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
