// JsonParser.cs
using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace MayhemFamiliar
{
    static class ZoneType
    {
        public const string Revealed = "ZoneType_Revealed";
        public const string Suppressed = "ZoneType_Suppressed";
        public const string Pending = "ZoneType_Pending";
        public const string Command = "ZoneType_Command";
        public const string Stack = "ZoneType_Stack";
        public const string Battlefield = "ZoneType_Battlefield";
        public const string Exile = "ZoneType_Exile";
        public const string Limbo = "ZoneType_Limbo";
        public const string Hand = "ZoneType_Hand";
        public const string Library = "ZoneType_Library";
        public const string Graveyard = "ZoneType_Graveyard";
        public const string Sideboard = "ZoneType_Sideboard";
    }
    static class ZoneId
    {
        public const int YourRevealed = 18;
        public const int OpponentsRevealed = 19;
        public const int Suppressed = 24;
        public const int Pending = 25;
        public const int Command = 26;
        public const int Stack = 27;
        public const int Battlefield = 28;
        public const int Exile = 29;
        public const int Limbo = 30;
        public const int YourHand = 31;
        public const int YourLibrary = 32;
        public const int YourGraveyard = 33;
        public const int YourSideboard = 34;
        public const int OpponentsHand = 35;
        public const int OpponentsLibrary = 36;
        public const int OpponentsGraveyard = 37;
        public const int OpponentsSideboard = 38;
        public static List<int> YourZones = [
            YourRevealed, YourHand, YourLibrary, YourGraveyard, YourSideboard
        ];
        public static List<int> OpponentsZones = [
            OpponentsRevealed, OpponentsHand, OpponentsLibrary, OpponentsGraveyard, OpponentsSideboard
        ];
    }
    static class GreMessageType
    {
        public const string ConnectResp = "GREMessageType_ConnectResp";
        public const string DieRollResultsResp = "GREMessageType_DieRollResultsResp";
        public const string GameStateMessage = "GREMessageType_GameStateMessage";
        public const string SetSettingsResp = "GREMessageType_SetSettingsResp";
        public const string PromptReq = "GREMessageType_PromptReq";
        public const string MulliganReq = "GREMessageType_MulliganReq";
    }
    static class GameStateType
    {
        public const string Full = "GameStateType_Full";
        public const string Diff = "GameStateType_Diff";
    }
    static class GameObjectType
    {
        public const string Card = "GameObjectType_Card";
    }
    static class GameStage
    {
        public const string Start = "GameStage_Start";
        public const string GameOver = "GameStage_GameOver";
    }
    static class AnnotationType
    {
        public const string NewTurnStarted = "AnnotationType_NewTurnStarted";
        public const string PhaseOrStepModified = "AnnotationType_PhaseOrStepModified";
        public const string ObjectIdChanged = "AnnotationType_ObjectIdChanged";
        public const string ZoneTransfer = "AnnotationType_ZoneTransfer";
        public const string UserActionTaken = "AnnotationType_UserActionTaken";
        public const string EnteredZoneThisTurn = "AnnotationType_EnteredZoneThisTurn";
        public const string ColorProduction = "AnnotationType_ColorProduction";
    }
    static class Key
    {
        public const string AbilityGrpId = "abilityGrpId";
        public const string ActionType = "actionType";
        public const string AffectedIds = "affectedIds";
        public const string AffectorId = "affectorId";
        public const string Annotations = "annotations";
        public const string Category = "category";
        public const string Details = "details";
        public const string DiffDeletedInstanceIds = "diffDeletedInstanceIds";
        public const string GameInfo = "gameInfo";
        public const string GameObjects = "gameObjects";
        public const string GameStateId = "gameStateId";
        public const string GameStateMessage = "gameStateMessage";
        public const string GreToClientEvent = "greToClientEvent";
        public const string GreToClientMessages = "greToClientMessages";
        public const string GrpId = "grpId";
        public const string InstanceId = "instanceId";
        public const string Name = "name";
        public const string NewId = "new_id";
        public const string ObjectInstanceIds = "objectInstanceIds";
        public const string OrigId = "orig_id";
        public const string Stage = "stage";
        public const string TransactionId = "transactionId";
        public const string Type = "type";
        public const string ValueInt32 = "valueInt32";
        public const string ValueString = "valueString";
        public const string ZoneId = "zoneId";
        public const string Zones = "zones";
        public const string ZoneDest = "zone_dest";
        public const string ZoneSrc = "zone_src";
    }
    public static class ZoneTransferCategory
    {
        public const string PlayLand = "PlayLand";
        public const string Mill = "Mill";
        public const string Resolve = "Resolve";
        public const string CastSpell = "CastSpell";
        public const string Draw = "Draw";
    }
    public static class Phase
    {
        public const string Beginning = "Phase_Beginning";
        public const string Main = "Phase_Main";
        public const string Main1 = "Phase_Main1";
        public const string Combat = "Phase_Combat";
        public const string Main2 = "Phase_Main2";
    }
    public static class Step
    {
        public const string Upkeep = "Step_Upkeep";

    }
    class GameObject
    {
        public int ZoneId { get; set; }
        public int GrpId { get; set; }
        public string Name { get; set; }
        public GameObject(int zoneId = -1, int grpId = -1, string name = "Unknown")
        {
            ZoneId = zoneId;
            GrpId = grpId;
            Name = name;
        }
    }
    class TurnInfo
    {
        public string Phase { get; set; } = "";
        public string Step { get; set; } = "";
        public int TurnNumber { get; set; } = -1;
        public int ActivePlayer { get; set; } = -1;
        public int PriorityPlayer { get; set; } = -1;
        public int DecisionPlayer { get; set; } = -1;
        public string NextPhase { get; set; } = "";
        public static string Key = "TurnInfo";
        public static string PhaseKey = "phase";
        public static string StepKey = "step";
        public static string TurnNumberKey = "turnNumber";
        public static string ActivePlayerKey = "activePlayer";
        public static string PriorityPlayerKey = "priorityPlayer";
        public static string DecisionPlayerKey = "decisionPlayer";
        public static string NextPhaseKey = "nextPhase";
    }
    internal class JsonParser
    {
        private readonly Action<string>? _log;
        private Dictionary<int, GameObject> _gameObjects;
        private string _cardDatabaseFilePath;
        private string _cardDatabaseDirectoryPath = @"C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw";
        private CardData _cardData;
        private TurnInfo _turnInfo = new TurnInfo();
        private int _gameStateId = 0;
        private readonly Dictionary<int, string> _UserActions;
        private readonly Dictionary<int, string> _Affectors;

        public JsonParser(Action<string> log, string cardDatabaseDirectoryPath = "")
        {
            _log = log;
            _gameObjects = new Dictionary<int, GameObject>();

            _UserActions[3] = "PlayLand";
            _UserActions[4] = "Tap";

            _Affectors[1] = "Unknown";
            _Affectors[1] = "You";
            _Affectors[2] = "Opponent";

            if (!String.IsNullOrEmpty(cardDatabaseDirectoryPath))
            {
                _cardDatabaseDirectoryPath = cardDatabaseDirectoryPath;
            }
            if (!Directory.Exists(_cardDatabaseDirectoryPath))
            {
                throw new DirectoryNotFoundException($"カードデータベースディレクトリが見つかりません: {_cardDatabaseDirectoryPath}");
            }
            string? cardDatabaseFileName = GetCardDatabaseFileName(_cardDatabaseDirectoryPath);
            if (String.IsNullOrEmpty(cardDatabaseFileName))
            {
                throw new FileNotFoundException("カードデータベースファイルが見つかりません。ディレクトリ内にRaw_CardDatabase_*.mtgaファイルが存在することを確認してください。", _cardDatabaseDirectoryPath);
            }
            _cardDatabaseFilePath = Path.Combine(_cardDatabaseDirectoryPath, GetCardDatabaseFileName(_cardDatabaseDirectoryPath));
            _log?.Invoke("JsonParser: CardDatabase接続開始");
            _cardData = new CardData(_cardDatabaseFilePath);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _log?.Invoke("JsonParser: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (JsonQueue.Queue.TryDequeue(out string jsonString))
                    {
                        ProcessJson(jsonString);
                    }
                    else
                    {
                        // キューが空なら短い間隔で待機（ブロック）
                        await Task.Delay(100, cancellationToken);
                    }
                }
                _log?.Invoke("JsonParser: キャンセルされました");
            }
            catch (OperationCanceledException)
            {
                _log?.Invoke("JsonParser: キャンセルされました");
            }
            catch (Exception ex)
            {
                _log?.Invoke($"JsonParser: エラー発生: {ex.Message}");
            }
        }

        private void ProcessJson(string jsonString)
        {
            // JSONパース
            JObject json = JObject.Parse(jsonString);

            // 条件チェック
            if (json[Key.TransactionId] is null)
            {
                return;
            }

            // GreToClientMessages
            if (json[Key.GreToClientEvent]?[Key.GreToClientMessages] is not null)
            {
                foreach (var message in json[Key.GreToClientEvent][Key.GreToClientMessages])
                {
                    ProcesseGreToClientMessage(message);
                }
            }

            // 
            if (json[Key.GreToClientEvent]?[Key.GreToClientMessages] is not null)
            {
                foreach (var message in json[Key.GreToClientEvent][Key.GreToClientMessages])
                {
                    ProcesseGreToClientMessage(message);
                }
            }

        }

        private string ProcesseGreToClientMessage(JToken message)
        {
            switch (message[Key.Type]?.ToString())
            {
                case GreMessageType.MulliganReq:
                    // GREMessageType_MulliganReq
                    _log?.Invoke("JsonParser: GREMessageType_MulliganReq");
                    break;
                case GreMessageType.GameStateMessage:
                    switch (message[Key.GameStateMessage]?[Key.Type]?.ToString())
                    {
                        case GameStateType.Full:
                            ProcesseGameStateTypeFullMessage(message[Key.GameStateMessage]);
                            break;
                        case GameStateType.Diff:
                            ProcesseGameStateTypeDiffMessage(message[Key.GameStateMessage]);
                            break;
                    }
                    break;
            }
            return "";
        }
        private void ProcesseGameStateTypeFullMessage(JToken message)
        {

            _gameStateId = (int)message[Key.GameStateId];

            // ゲーム開始連絡
            if (message[Key.GameInfo]?[Key.Stage]?.ToString() == GameStage.Start)
            {
                _log?.Invoke("JsonParser: ゲーム開始");
            }

            // Zonesの処理（gameObjectsにoiidを登録）
            processZones(message[Key.Zones]?.ToArray() ?? Array.Empty<JToken>());
        }
        private void ProcesseGameStateTypeDiffMessage(JToken message)
        {
            // TODO

            _gameStateId = (int)message[Key.GameStateId];

            // turnInfoの処理（TurnInfoの更新、ターンの開始を判定）
            processTurnInfo(message[TurnInfo.Key]);

            // gameObjectsの処理（gameObjectsにoiidを登録、更新）
            processGameObjects(message[Key.GameObjects].ToArray() ?? Array.Empty<JToken>());

            // annotationsの処理
            processAnnotations(message[Key.Annotations]?.ToArray() ?? Array.Empty<JToken>());

            // ゲーム終了連絡
            if (message[Key.GameInfo]?[Key.Stage]?.ToString() == GameStage.GameOver)
            {
                _log?.Invoke("JsonParser: ゲーム終了");
            }
        }

        private void processAnnotations(IList<JToken> annotations)
        {
            foreach (var annotation in annotations)
            {
                // TODO
                switch (annotation[Key.Type]?[0]?.ToString())
                {
                    case AnnotationType.ObjectIdChanged
                        when annotation[Key.Details] is not null:
                        {
                            // オブジェクトIDの変更
                            int origId = (int)annotation[Key.Details][0][Key.ValueInt32][0];
                            int newId = (int)annotation[Key.Details][1][Key.ValueInt32][0];
                            _gameObjects[newId] = new GameObject(_gameObjects[origId].ZoneId, _gameObjects[origId].GrpId, _gameObjects[origId].Name);
                            break;
                        }
                    case AnnotationType.ZoneTransfer
                        when annotation[Key.Details] is not null:
                        {
                            // ゾーンの移動
                            int affectedId = (int)annotation[Key.AffectedIds][0];
                            int zoneSrcId = (int)annotation[Key.Details][0][Key.ValueInt32][0];
                            int zoneDestId = (int)annotation[Key.Details][1][Key.ValueInt32][0];
                            string zoneTransferCategory = annotation[Key.Details][2][Key.ValueInt32][0].ToString();
                            string affector;
                            if (ZoneId.YourZones.Contains(zoneSrcId) && ZoneId.YourZones.Contains(zoneDestId)) 
                            {
                                affector = _Affectors[1]; // あなた
                            } 
                            else if (ZoneId.OpponentsZones.Contains(zoneSrcId) && ZoneId.OpponentsZones.Contains(zoneDestId))
                            {
                                affector = _Affectors[2]; // 相手
                            } 
                            else
                            {
                                affector = _Affectors[0]; // 不明
                            }
                            if (_gameObjects.ContainsKey(affectedId))
                            {
                                _gameObjects[affectedId].ZoneId = zoneDestId;
                            }
                            _log?.Invoke($"JsonParser: {affector} {zoneTransferCategory} {_gameObjects[affectedId].Name}");
                            // TODO: ドローや土地のプレイや呪文のキャストはここで処理
                            EventQueue.Queue.Enqueue(
                                $"{affector} {zoneTransferCategory} {_gameObjects[affectedId].Name}"
                            );
                            break;
                        }
                    case AnnotationType.UserActionTaken
                        when annotation[Key.Details] is not null:
                        {
                            // ユーザアクション
                            string affector = _Affectors[(int)annotation[Key.AffectorId]];
                            int affectedId = (int)annotation[Key.AffectedIds][0];
                            int actionType = (int)annotation[Key.Details][0][Key.ValueInt32][0];
                            int abilityGrpId = (int)annotation[Key.Details][1][Key.ValueInt32][0];
                            // TODO
                            switch (actionType)
                            {
                                case 3: // PlayLand
                                    _log?.Invoke($"JsonParser: {affector} {ZoneTransferCategory.PlayLand} {_gameObjects[affectedId].Name}");
                                    break;
                                case 4: // Tap
                                    _log?.Invoke($"JsonParser: {affector} Tap {_gameObjects[affectedId].Name}");
                                    break;
                            }

                            break;
                        }
                }
            }
        }

        private void processTurnInfo(JToken gameInfo)
        {
            if (gameInfo[TurnInfo.Key] is not null)
            {
                // チェック
                if (gameInfo[TurnInfo.Key][TurnInfo.PhaseKey]?.ToString() == Phase.Beginning && 
                    gameInfo[TurnInfo.Key][TurnInfo.StepKey]?.ToString() == Step.Upkeep &&
                    _turnInfo.Phase != Phase.Beginning) 
                {
                    _log.Invoke("JsonParser: ターン開始 - " + _turnInfo.TurnNumber);
                }

                // 更新
                _turnInfo.Phase = gameInfo[TurnInfo.Key][TurnInfo.PhaseKey]?.ToString() ?? _turnInfo.Phase;
                _turnInfo.Step = gameInfo[TurnInfo.Key][TurnInfo.StepKey]?.ToString() ?? _turnInfo.Step;
                _turnInfo.TurnNumber = (int)(gameInfo[TurnInfo.Key][TurnInfo.TurnNumberKey] ?? _turnInfo.TurnNumber);
                _turnInfo.ActivePlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.ActivePlayerKey] ?? _turnInfo.ActivePlayer);
                _turnInfo.PriorityPlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.PriorityPlayerKey] ?? _turnInfo.PriorityPlayer);
                _turnInfo.DecisionPlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.DecisionPlayerKey] ?? _turnInfo.DecisionPlayer);
                _turnInfo.NextPhase = gameInfo[TurnInfo.Key][TurnInfo.NextPhaseKey]?.ToString() ?? _turnInfo.NextPhase;
            }
        }

        private void processZones(IList<JToken> zones)
        {
            foreach (var zone in zones)
            {
                if (zone[Key.ObjectInstanceIds] is not null)
                {
                    foreach (int oiid in zone[Key.ObjectInstanceIds])
                    {
                        if (!_gameObjects.ContainsKey(oiid))
                        {
                            _gameObjects[oiid] = new GameObject((int)zone[Key.ZoneId]);
                        }
                    }
                }
            }
        }

        private void processGameObjects(IList<JToken> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                int oiid = (int)gameObject[Key.InstanceId];
                if (!_gameObjects.ContainsKey(oiid))
                {
                    // オブジェクトが存在しなければ追加
                    _gameObjects[oiid] = new GameObject();
                }
                // オブジェクトを更新
                _gameObjects[oiid].GrpId = (int)gameObject[Key.GrpId];
                _gameObjects[oiid].ZoneId = (int)gameObject[Key.ZoneId];
                if (gameObject[Key.Name] is not null)
                {
                    _gameObjects[oiid].Name = _cardData.GetCardNameByLocId((int)gameObject[Key.Name]) ?? "Unknown";
                }
                else if (_cardData != null && _gameObjects[oiid].GrpId != -1)
                {
                    _gameObjects[oiid].Name = _cardData.GetCardNameByGrpId(_gameObjects[oiid].GrpId) ?? "Unknown";
                }
            }
        }

        private static string? GetCardDatabaseFileName(string cardDatabaseDirectoryPath)
        {
            try
            {
                // ディレクトリが存在しない場合はnullを返す
                if (!Directory.Exists(cardDatabaseDirectoryPath))
                {
                    return null;
                }

                // 正規表現パターン
                string pattern = @"Raw_CardDatabase_.*\.mtga";

                // ディレクトリ内のファイルを検索し、正規表現にマッチするものを取得
                var cardDatabaseFile = Directory
                    .GetFiles(cardDatabaseDirectoryPath)
                    .Where(file => Regex.IsMatch(Path.GetFileName(file), pattern))
                    .OrderByDescending(file => new FileInfo(file).LastWriteTimeUtc)
                    .FirstOrDefault();

                // マッチするファイルがあればそのファイル名を、なければnullを返す
                return cardDatabaseFile != null ? Path.GetFileName(cardDatabaseFile) : null;
            }
            catch (Exception)
            {
                // 例外が発生した場合もnullを返す
                return null;
            }
        }
    }
}