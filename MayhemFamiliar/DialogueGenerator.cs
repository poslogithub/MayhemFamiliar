using MayhemFamiliar.QueueManager;
using System.Globalization;

namespace MayhemFamiliar
{
    class Verb
    {
        public const string Mulligan = "Mulligan";
        public const string GameStart = "GameStart";
        public const string GameOver = "GameOver";
        public const string TurnStart = "TurnStart";
        public static readonly string[] Speak = {
            ZoneTransferCategory.Discard,
            ZoneTransferCategory.PlayLand,
            ZoneTransferCategory.CastSpell,
            ZoneTransferCategory.Draw,
            ZoneTransferCategory.Sacrifice,
            Verb.Mulligan,
            Verb.GameStart,
            Verb.GameOver,
            Verb.TurnStart
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

            string dialogue = "";
            switch(verb)
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
                case Verb.TurnStart:
                    switch (subject)
                    {
                        case Player.You:
                            dialogue = "こちらのターン。";
                            break;
                        case Player.Opponent:
                            dialogue = "お相手のターン。";
                            break;
                        default:
                            dialogue = "誰かのターン。";
                            break;
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
                case Player.You:
                    break;
                case Player.Opponent:
                    dialogue = "お相手が";
                    break;
                default:
                    dialogue = "不明なプレイヤーが";
                    break;
            }
            if (!String.IsNullOrEmpty(objective))
            {
                if (!(subject == Player.Opponent && verb == ZoneTransferCategory.Draw))
                {
                    objective = ReplaceObjectiveDelimiters(objective);
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
                    dialogue += "生け贄。";
                    break;
                default:
                    dialogue += "不明な動作。";
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
            string[] words = eventString.Split(' ', 3);

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
            }
            return objective;
        }
    }
}
