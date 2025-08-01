// JsonParser.cs
using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    static class  MatchState
    {
        public const string Key = "matchState";
        public const string GameComplete = "MatchState_GameComplete";
        public const string MatchComplete = "MatchState_MatchComplete";
        public const string GameInProgress = "MatchState_GameInProgress";
    }
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
        public static List<int> YourZones = new List<int>() {
            YourRevealed, YourHand, YourLibrary, YourGraveyard, YourSideboard
        };
        public static List<int> OpponentsZones = new List<int>() {
            OpponentsRevealed, OpponentsHand, OpponentsLibrary, OpponentsGraveyard, OpponentsSideboard
        };
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
        public const string Ability = "GameObjectType_Ability";
        public const string Token = "GameObjectType_Token";
        public const string MDFCBack = "GameObjectType_MDFCBack";
        public static List<string> NamableObjectTypes = new List<string>() {
            Card, Token, MDFCBack
        };
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
        public const string ResolutionStart = "AnnotationType_ResolutionStart";
        public const string ResolutionComplete = "AnnotationType_ResolutionComplete";
    }
    static class Key
    {
        public const string AbilityGrpId = "abilityGrpId";
        public const string ActionType = "actionType";
        public const string AffectedIds = "affectedIds";
        public const string AffectorId = "affectorId";
        public const string Annotations = "annotations";
        public const string Category = "category";
        public const string ControllerSeatId = "controllerSeatId";
        public const string Details = "details";
        public const string DiffDeletedInstanceIds = "diffDeletedInstanceIds";
        public const string GameInfo = "gameInfo";
        public const string GameObjects = "gameObjects";
        public const string GameStateId = "gameStateId";
        public const string GameStateMessage = "gameStateMessage";
        public const string GreToClientEvent = "greToClientEvent";
        public const string GreToClientMessages = "greToClientMessages";
        public const string GrpId = "grpId";
        public const string Id = "id";
        public const string InstanceId = "instanceId";
        public const string Name = "name";
        public const string NewId = "new_id";
        public const string ObjectInstanceIds = "objectInstanceIds";
        public const string OrigId = "orig_id";
        public const string OwnerSeatId = "ownerSeatId";
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
    static class Phase
    {
        public const string Beginning = "Phase_Beginning";
        public const string Main = "Phase_Main";
        public const string Main1 = "Phase_Main1";
        public const string Combat = "Phase_Combat";
        public const string Main2 = "Phase_Main2";
    }
    static class Step
    {
        public const string Upkeep = "Step_Upkeep";

    }
    static class Visibility
    {
        public const string Key = "visibility";
        public const string Public = "Visibility_Public";
        public const string Private = "Visibility_Private";
        public const string Hidden = "Visibility_Hidden";
    }
    static class Unknown
    {
        public const string Name = "Unknown_Name";
        public const string Type = "Unknown_Type";
        public const int GrpId = 0;
        public const int ZoneId = 0;
        public const string Player = "Unknown_Player";
        public const int OwnerSeatId = 0;
        public const int ControllerSeatId = 0;
        public const int Id = 0;
    }
    static class PlayerId
    {
        public const int Unknown = 0; // ゲーム開始前や不明なプレイヤー
        public const int You = 1;
        public const int Opponent = 2;
    }
    public static class Player
    {
        public const string Unknown = "Unknown_Player"; // ゲーム開始前や不明なプレイヤー
        public const string You = "You";
        public const string Opponent = "Opponent";
        public static readonly string[] ById = new[] { Unknown, You, Opponent };
    }
    public static class ZoneTransferCategory
    {
        public const string CastSpell = "CastSpell";
        public const string Damage = "SBA_Damage"; // クリーチャーへのダメージ
        public const string Discard = "Discard";
        public const string Draw = "Draw";
        public const string Exile = "Exile";
        public const string Mill = "Mill";
        public const string Nil = "nil";   // Fizzった
        public const string PlayLand = "PlayLand";
        public const string Sacrifice = "Sacrifice";
        public const string Resolve = "Resolve";
        public const string Return = "Return";
    }
    class GameObject
    {
        public int GrpId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int ZoneId { get; set; }
        public string Visible { get; set; } // Visibilityクラスと名前が被ってしまうので苦肉の策
        public int OwnerSeatId { get; set; }
        public int ControllerSeatId { get; set; }
        public GameObject(
            int grpId = Unknown.GrpId,
            string name = Unknown.Name,
            string type = Unknown.Type,
            int zoneId = Unknown.ZoneId,
            string visibility = Visibility.Hidden,
            int ownerSeatId = Unknown.OwnerSeatId,
            int controllerSeatId = Unknown.ControllerSeatId)
        {
            GrpId = grpId;
            Name = name;
            Type = type;
            ZoneId = zoneId;
            Visible = visibility;
            ControllerSeatId = controllerSeatId;
            OwnerSeatId = ownerSeatId;
        }
        public string GetDescription()
        {
            return $"GrpId: {GrpId}, Name: \"{Name}\", Type: {Type}, ZoneId: {ZoneId}, Visible: {Visible}, OwnerSeatId: {OwnerSeatId}, ControllerSeatId: {ControllerSeatId}";
        }
    }
    class TurnInfo
    {
        public string Phase { get; set; } = "";
        public string Step { get; set; } = "";
        public int TurnNumber { get; set; } = 0;
        public int ActivePlayer { get; set; } = PlayerId.Unknown;
        public int PriorityPlayer { get; set; } = PlayerId.Unknown;
        public int DecisionPlayer { get; set; } = PlayerId.Unknown;
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
        private readonly string _cardDatabaseFile;
        private Dictionary<int, GameObject> _gameObjects;
        private CardData _cardData;
        private TurnInfo _turnInfo = new TurnInfo();
        private int _gameStateId = 0;

        public JsonParser(string cardDatabaseFile)
        {
            _cardDatabaseFile = cardDatabaseFile;
            _gameObjects = new Dictionary<int, GameObject>();
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            _cardData = new CardData(_cardDatabaseFile);
            Logger.Instance.Log($"{this.GetType().Name}: CardDatabase接続");

            Logger.Instance.Log($"{this.GetType().Name}: 開始");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
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
                catch (OperationCanceledException)
                {
                    Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"{this.GetType().Name}: エラー発生: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private void ProcessJson(string jsonString)
        {
            // JSONパース
            JObject json = null;
            try
            {
                json = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: JSON文字列のパース中に例外が発生", LogLevel.Error);
                Logger.Instance.Log($"{this.GetType().Name}: {ex.Message}", LogLevel.Error);
                return;
            }

            // TransactionIdが無いメッセージは全て無視
            if (json?[Key.TransactionId] is null)
            {
                return;
            }

            // GreToClientMessages
            if (json[Key.GreToClientEvent]?[Key.GreToClientMessages] != null)
            {
                foreach (var message in json[Key.GreToClientEvent][Key.GreToClientMessages])
                {
                    ProcesseGreToClientMessage(message);
                }
            }
        }

        private void ProcesseGreToClientMessage(JToken message)
        {
            switch (message[Key.Type]?.ToString())
            {
                case GreMessageType.MulliganReq:
                    // GREMessageType_MulliganReq
                    Logger.Instance.Log($"{this.GetType().Name}: GREMessageType_MulliganReq");
                    EventQueue.Queue.Enqueue($"{Player.You} {Verb.Mulligan}");
                    break;
                case GreMessageType.GameStateMessage:
                    _gameStateId = (int)message[Key.GameStateId];
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
        }
        private void ProcesseGameStateTypeFullMessage(JToken message)
        {
            // GameObjectの初期化
            _gameObjects.Clear();

            // turnInfoの処理（TurnInfoの更新、ターンの開始を判定）
            processTurnInfo(message[TurnInfo.Key]);

            // gameInfoの処理
            processGameInfo(message[Key.GameInfo]);

            // Zonesの処理（gameObjectsにoiidを登録）
            processZones(message[Key.Zones]?.ToArray() ?? Array.Empty<JToken>());
        }
        private void ProcesseGameStateTypeDiffMessage(JToken message)
        {
            // turnInfoの処理（TurnInfoの更新、ターンの開始を判定）
            processTurnInfo(message[TurnInfo.Key]);

            // Zonesの処理（gameObjectsにoiidを登録）
            processZones(message[Key.Zones]?.ToArray() ?? Array.Empty<JToken>());

            // gameObjectsの処理（gameObjectsにoiidを登録、更新）
            processGameObjects(message[Key.GameObjects]?.ToArray() ?? Array.Empty<JToken>());

            // annotationsの処理
            processAnnotations(message[Key.Annotations]?.ToArray() ?? Array.Empty<JToken>());

            // gameInfoの処理
            processGameInfo(message[Key.GameInfo]);
        }

        private void processGameInfo(JToken gameInfo)
        {
            if (gameInfo is null) return;

            // ゲーム開始連絡
            if (gameInfo[Key.Stage]?.ToString() == GameStage.Start)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ゲーム開始");
                EventQueue.Queue.Enqueue($"{Player.You} {Verb.GameStart}");
            }

            // ゲーム終了連絡
            if (gameInfo[Key.Stage]?.ToString() == GameStage.GameOver &&
                gameInfo[MatchState.Key]?.ToString() == MatchState.GameComplete)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ゲーム終了");
                EventQueue.Queue.Enqueue($"{Player.You} {Verb.GameOver}");
            }
        }
        private void processAnnotations(IList<JToken> annotations)
        {
            if (annotations is null) return;

            foreach (var annotation in annotations)
            {
                switch (annotation[Key.Type]?[0]?.ToString())
                {
                    case AnnotationType.ObjectIdChanged
                        when annotation[Key.Details] != null:
                        {
                            // オブジェクトIDの変更
                            int origId = (int)annotation[Key.Details][0][Key.ValueInt32][0];
                            int newId = (int)annotation[Key.Details][1][Key.ValueInt32][0];
                            if (!_gameObjects.ContainsKey(newId))
                            {
                                _gameObjects[newId] = new GameObject(
                                    _gameObjects[origId].GrpId, 
                                    _gameObjects[origId].Name, 
                                    _gameObjects[origId].Type, 
                                    _gameObjects[origId].ZoneId, 
                                    _gameObjects[origId].Visible, 
                                    _gameObjects[origId].OwnerSeatId, 
                                    _gameObjects[origId].ControllerSeatId);
                                Logger.Instance.Log($"{this.GetType().Name}: オブジェクトID変更 - OrigId: {origId}, NewId: {newId}, {_gameObjects[newId].GetDescription()}", LogLevel.Debug);
                            }
                            // ProcessZoneと競合するので、存在しない時しかやらない
                            break;
                        }
                    case AnnotationType.ZoneTransfer
                        when annotation[Key.Details] != null:
                        {
                            // ゾーンの移動
                            int id = (int)(annotation[Key.Id] ?? Unknown.Id);
                            int affectorId = (int)(annotation[Key.AffectorId] ?? Unknown.Id);
                            int[] affectedIds = annotation[Key.AffectedIds]?.ToObject<int[]>() ?? Array.Empty<int>();
                            int zoneSrcId = (int)(annotation[Key.Details][0]?[Key.ValueInt32]?[0] ?? Unknown.ZoneId);
                            int zoneDestId = (int)(annotation[Key.Details][1]?[Key.ValueInt32]?[0] ?? Unknown.ZoneId);
                            string zoneTransferCategory = annotation[Key.Details]?[2]?[Key.ValueString]?[0].ToString();
                            if (affectorId == Unknown.Id)
                            {
                                if (ZoneId.YourZones.Contains(zoneSrcId) || ZoneId.YourZones.Contains(zoneDestId))
                                {
                                    affectorId = PlayerId.You; // あなた
                                }
                                else if (ZoneId.OpponentsZones.Contains(zoneSrcId) || ZoneId.OpponentsZones.Contains(zoneDestId))
                                {
                                    affectorId = PlayerId.Opponent; // 対戦相手
                                }
                                else
                                {
                                    affectorId = PlayerId.Unknown; // 不明
                                }
                            }
                            // 【改善可能】affectorIdが10未満ならプレイヤー、そうでないならGameObjectのcontrollerかownerとする。
                            // 10は仮。いずれプレイヤー数が増える可能性があるので。
                            string player = affectorId < 10 ? Player.ById[affectorId] : 
                                (_gameObjects[affectorId].ControllerSeatId != Unknown.ControllerSeatId ? Player.ById[_gameObjects[affectorId].ControllerSeatId] :
                                    (_gameObjects[affectorId].OwnerSeatId != Unknown.OwnerSeatId ? Player.ById[_gameObjects[affectorId].OwnerSeatId] : Unknown.Player)
                                );
                            foreach (int affectedId in affectedIds)
                            {
                                if (!_gameObjects.ContainsKey(affectedId))
                                {
                                    _gameObjects[affectedId].ZoneId = zoneDestId;
                                }
                                Logger.Instance.Log($"{this.GetType().Name}: {player} {zoneTransferCategory} \"{_gameObjects[affectedId].Name}\"", LogLevel.Debug);
                                EventQueue.Queue.Enqueue($"{player} {zoneTransferCategory} \"{_gameObjects[affectedId].Name}\"");
                            }
                            break;
                        }
                }
            }
        }

        private void processTurnInfo(JToken gameInfo)
        {
            if (gameInfo?[TurnInfo.Key] != null)
            {
                // チェック
                if (gameInfo[TurnInfo.Key][TurnInfo.PhaseKey]?.ToString() == Phase.Beginning && 
                    _turnInfo.Phase != Phase.Beginning) 
                {
                    // TODO: 誰の？
                    Logger.Instance.Log($"{this.GetType().Name}: ターン開始 - " + _turnInfo.TurnNumber);
                    EventQueue.Queue.Enqueue($"{Player.Unknown} {Verb.TurnStart}");
                }

                // 更新
                _turnInfo.Phase = gameInfo[TurnInfo.Key][TurnInfo.PhaseKey]?.ToString() ?? _turnInfo.Phase;
                _turnInfo.Step = gameInfo[TurnInfo.Key][TurnInfo.StepKey]?.ToString() ?? _turnInfo.Step;
                _turnInfo.TurnNumber = (int)(gameInfo[TurnInfo.Key][TurnInfo.TurnNumberKey] ?? _turnInfo.TurnNumber);
                _turnInfo.ActivePlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.ActivePlayerKey] ?? _turnInfo.ActivePlayer);
                _turnInfo.PriorityPlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.PriorityPlayerKey] ?? _turnInfo.PriorityPlayer);
                _turnInfo.DecisionPlayer = (int)(gameInfo[TurnInfo.Key][TurnInfo.DecisionPlayerKey] ?? _turnInfo.DecisionPlayer);
                _turnInfo.NextPhase = gameInfo[TurnInfo.Key][TurnInfo.NextPhaseKey]?.ToString() ?? _turnInfo.NextPhase;

                Logger.Instance.Log($"{this.GetType().Name}: ターン情報更新 - " +
                    $"Phase: {_turnInfo.Phase}, Step: {_turnInfo.Step}, " +
                    $"TurnNumber: {_turnInfo.TurnNumber}, " +
                    $"ActivePlayer: {_turnInfo.ActivePlayer}, " +
                    $"PriorityPlayer: {_turnInfo.PriorityPlayer}, " +
                    $"DecisionPlayer: {_turnInfo.DecisionPlayer}, " +
                    $"NextPhase: {_turnInfo.NextPhase}", 
                    LogLevel.Debug);
            }
        }

        private void processZones(IList<JToken> zones)
        {
            if (zones is null) return;

            foreach (var zone in zones)
            {
                if (zone[Key.ObjectInstanceIds] != null)
                {
                    foreach (int oiid in zone[Key.ObjectInstanceIds])
                    {
                        if (!_gameObjects.ContainsKey(oiid))
                        {
                            _gameObjects[oiid] = new GameObject(zoneId: (int)zone[Key.ZoneId]);
                            Logger.Instance.Log($"{this.GetType().Name}: オブジェクト新規登録 - InstanceId: {oiid}, ZoneId: {(int)zone[Key.ZoneId]}", LogLevel.Debug);
                        }
                        else if (_gameObjects[oiid].ZoneId != (int)zone[Key.ZoneId])
                        {
                            // 既存のオブジェクトのゾーンが異なる場合は更新
                            Logger.Instance.Log($"{this.GetType().Name}: オブジェクトゾーン更新 - InstanceId: {oiid}, OldZoneId: {_gameObjects[oiid].ZoneId}, NewZoneId: {(int)zone[Key.ZoneId]}, {_gameObjects[oiid].GetDescription()}", LogLevel.Debug);
                            _gameObjects[oiid].ZoneId = (int)zone[Key.ZoneId];
                        }
                    }
                }
            }
        }

        private void processGameObjects(IList<JToken> gameObjects)
        {
            if (gameObjects is null) return;

            foreach (var gameObject in gameObjects)
            {
                int instanceId = (int)gameObject[Key.InstanceId];
                string type = gameObject[Key.Type]?.ToString() ?? Unknown.Type;
                if (!_gameObjects.ContainsKey(instanceId))
                {
                    // オブジェクトが存在しなければ追加
                    _gameObjects[instanceId] = new GameObject();
                    Logger.Instance.Log($"{this.GetType().Name}: オブジェクト新規登録 - InstanceId: {instanceId}", LogLevel.Debug);
                }
                // オブジェクトを更新
                _gameObjects[instanceId].GrpId = (int)(gameObject[Key.GrpId] ?? _gameObjects[instanceId].GrpId);
                _gameObjects[instanceId].Type = gameObject[Key.Type]?.ToString() ?? _gameObjects[instanceId].Type;
                _gameObjects[instanceId].ZoneId = (int)(gameObject[Key.ZoneId] ?? _gameObjects[instanceId].ZoneId);
                _gameObjects[instanceId].Visible = gameObject[Visibility.Key]?.ToString() ?? _gameObjects[instanceId].Visible;
                _gameObjects[instanceId].OwnerSeatId = (int)(gameObject[Key.OwnerSeatId] ?? _gameObjects[instanceId].OwnerSeatId);
                _gameObjects[instanceId].ControllerSeatId = (int)(gameObject[Key.ControllerSeatId] ?? _gameObjects[instanceId].ControllerSeatId);
                if (gameObject[Key.Name] != null)
                {
                    _gameObjects[instanceId].Name = _cardData.GetCardNameByLocId((int)gameObject[Key.Name]) ?? _gameObjects[instanceId].Name;
                }
                else if (_gameObjects[instanceId].GrpId != Unknown.GrpId && GameObjectType.NamableObjectTypes.Contains(_gameObjects[instanceId].Type))
                {
                    _gameObjects[instanceId].Name = _cardData.GetCardNameByGrpId(_gameObjects[instanceId].GrpId) ?? _gameObjects[instanceId].Name;
                }
                else if (!GameObjectType.NamableObjectTypes.Contains(_gameObjects[instanceId].Type))
                {
                    _gameObjects[instanceId].Name = _gameObjects[instanceId].Type.Replace("GameObjectType_", "");
                }
                Logger.Instance.Log($"{this.GetType().Name}: オブジェクト更新 - InstanceId: {instanceId}, {_gameObjects[instanceId].GetDescription()}", LogLevel.Debug);
            }
        }


    }
}