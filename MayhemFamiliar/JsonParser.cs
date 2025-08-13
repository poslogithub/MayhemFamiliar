// JsonParser.cs
using MayhemFamiliar.QueueManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
    static class PlayerStatus
    {
        public const string InGame = "PlayerStatus_InGame";
    }
    static class ControllerType
    {
        public const string Player = "ControllerType_Player";
    }
    public static class ZoneId
    {
        public static Dictionary<int, int> Revealed = new Dictionary<int, int>()
        {
            { 1, 18 },
            { 2, 19 }
        };
        public const int Suppressed = 24;
        public const int Pending = 25;
        public const int Command = 26;
        public const int Stack = 27;
        public const int Battlefield = 28;
        public const int Exile = 29;
        public const int Limbo = 30;
        public const int Hand1 = 31;
        public const int Hand2 = 35;
        public const int Library1 = 32;
        public const int Library2 = 36;
        public const int Graveyard1 = 33;
        public const int Graveyard2 = 37;
        public const int Sideboard1 = 34;
        public const int Sideboard2 = 38;
        public static Dictionary<int, int> Hand = new Dictionary<int, int>()
        {
            { 1, Hand1 },
            { 2, Hand2 }
        };
        public static Dictionary<int, int> Library = new Dictionary<int, int>()
        {
            { 1, Library1 },
            { 2, Library2 }
        };
        public static Dictionary<int, int> Graveyard = new Dictionary<int, int>()
        {
            { 1, Graveyard1 },
            { 2, Graveyard2 }
        };
        public static Dictionary<int, int> Sideboard = new Dictionary<int, int>()
        {
            { 1, Sideboard1 },
            { 2, Sideboard2 }
        };
        public static Dictionary<int, List<int>> PlayerZones = new Dictionary<int, List<int>>()
        {
            { 1, new List<int>() { Revealed[1], Hand[1], Library[1], Graveyard[1], Sideboard[1] } },
            { 2, new List<int>() { Revealed[2], Hand[2], Library[2], Graveyard[2], Sideboard[2] } }
        };
        public static readonly Dictionary<int, string> ZoneTypes = new Dictionary<int, string>()
        {
            { Revealed[1], "ZoneType_Revealed" },
            { Revealed[2], "ZoneType_Revealed" },
            { Suppressed, "ZoneType_Suppressed" },
            { Pending, "ZoneType_Pending" },
            { Command, "ZoneType_Command" },
            { Stack, "ZoneType_Stack" },
            { Battlefield, "ZoneType_Battlefield" },
            { Exile, "ZoneType_Exile" },
            { Limbo, "ZoneType_Limbo" },
            { Hand[1], "ZoneType_Hand" },
            { Hand[2], "ZoneType_Hand" },
            { Library[1], "ZoneType_Library" },
            { Library[2], "ZoneType_Library" },
            { Graveyard[1], "ZoneType_Graveyard" },
            { Graveyard[2], "GravZoneType_Graveyardeyard2" },
            { Sideboard[1], "ZoneType_Sideboard" },
            { Sideboard[2], "ZoneType_Sideboard" }
        };
    }
    static class GreMessageType
    {
        public const string GameStateMessage = "GREMessageType_GameStateMessage";
        public const string QueuedGameStateMessage = "GREMessageType_QueuedGameStateMessage";
        public const string MulliganReq = "GREMessageType_MulliganReq"; // 「マリガンチェック」

        public const string ActionsAvailableReq = "GREMessageType_ActionsAvailableReq";
        public const string ChooseStartingPlayerReq = "GREMessageType_ChooseStartingPlayerReq";
        public const string ConnectResp = "GREMessageType_ConnectResp";
        public const string DeclareAttackersReq = "GREMessageType_DeclareAttackersReq";
        public const string DeclareBlockersReq = "GREMessageType_DeclareBlockersReq";
        public const string DieRollResultsResp = "GREMessageType_DieRollResultsResp";
        public const string GroupReq = "GREMessageType_GroupReq";
        public const string IntermissionReq = "GREMessageType_IntermissionReq";
        public const string OptionalActionMessage = "GREMessageType_OptionalActionMessage";
        public const string PayCostsReq = "GREMessageType_PayCostsReq";
        public const string PromptReq = "GREMessageType_PromptReq";
        public const string SelectNReq = "GREMessageType_SelectNReq";
        public const string SelectTargetsReq = "GREMessageType_SelectTargetsReq";
        public const string SetSettingsResp = "GREMessageType_SetSettingsResp";
        public const string SubmitAttackersResp = "GREMessageType_SubmitAttackersResp"; // TODO: 「アタック」
        public const string SubmitBlockersResp = "GREMessageType_SubmitBlockersResp";   // TODO: 「ブロック」
        public const string SubmitTargetsResp = "GREMessageType_SubmitTargetsResp";
        public const string UIMessage = "GREMessageType_UIMessage";
        public static string[] IgnoreType =
        {
            ActionsAvailableReq,
            ChooseStartingPlayerReq,
            ConnectResp,
            DeclareAttackersReq,
            DeclareBlockersReq,
            DieRollResultsResp,
            GroupReq,
            IntermissionReq,
            OptionalActionMessage,
            PayCostsReq,
            PromptReq,
            SelectNReq,
            SubmitAttackersResp,
            SubmitBlockersResp,
            SelectTargetsReq,
            SetSettingsResp,
            SubmitTargetsResp,
            UIMessage,
        };
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
        public const string ModifiedLife = "AnnotationType_ModifiedLife";
        public const string NewTurnStarted = "AnnotationType_NewTurnStarted";
        public const string ObjectIdChanged = "AnnotationType_ObjectIdChanged";
        public const string TokenCreated = "AnnotationType_TokenCreated";
        public const string ZoneTransfer = "AnnotationType_ZoneTransfer";

        public const string AbilityInstanceCreated = "AnnotationType_AbilityInstanceCreated";
        public const string AbilityInstanceDeleted = "AnnotationType_AbilityInstanceDeleted";
        public const string AttachmentCreated = "AnnotationType_AttachmentCreated";
        public const string ChoiceResult = "AnnotationType_ChoiceResult";
        public const string ColorProduction = "AnnotationType_ColorProduction";
        public const string ControllerChanged = "AnnotationType_ControllerChanged";
        public const string CounterAdded = "AnnotationType_CounterAdded";
        public const string DamageDealt = "AnnotationType_DamageDealt";
        public const string EnteredZoneThisTurn = "AnnotationType_EnteredZoneThisTurn";
        public const string LayeredEffectCreated = "AnnotationType_LayeredEffectCreated";
        public const string LayeredEffectDestroyed = "AnnotationType_LayeredEffectDestroyed";
        public const string ManaPaid = "AnnotationType_ManaPaid";
        public const string MultistepEffectStarted = "AnnotationType_MultistepEffectStarted";
        public const string MultistepEffectComplete = "AnnotationType_MultistepEffectComplete";
        public const string PhaseOrStepModified = "AnnotationType_PhaseOrStepModified";
        public const string PlayerSelectingTargets = "AnnotationType_PlayerSelectingTargets";
        public const string PlayerSubmittedTargets = "AnnotationType_PlayerSubmittedTargets";
        public const string PowerToughnessModCreated = "AnnotationType_PowerToughnessModCreated";
        public const string ResolutionStart = "AnnotationType_ResolutionStart";
        public const string ResolutionComplete = "AnnotationType_ResolutionComplete";
        public const string RevealedCardCreated = "AnnotationType_RevealedCardCreated";
        public const string RevealedCardDeleted = "AnnotationType_RevealedCardDeleted";
        public const string ShouldntPlay = "AnnotationType_ShouldntPlay";
        public const string Shuffle = "AnnotationType_Shuffle";
        public const string SyntheticEvent = "AnnotationType_SyntheticEvent";
        public const string TappedUntappedPermanent = "AnnotationType_TappedUntappedPermanent";
        public const string TokenDeleted = "AnnotationType_TokenDeleted";
        public const string UserActionTaken = "AnnotationType_UserActionTaken";
        public static readonly string[] Ignore = new string[]
        {
            AbilityInstanceCreated,
            AbilityInstanceDeleted,
            AttachmentCreated,
            ChoiceResult,
            ColorProduction,
            ControllerChanged,
            CounterAdded,
            DamageDealt,
            EnteredZoneThisTurn,
            LayeredEffectCreated,
            LayeredEffectDestroyed,
            ManaPaid,
            MultistepEffectStarted,
            MultistepEffectComplete,
            PhaseOrStepModified,
            PlayerSelectingTargets,
            PlayerSubmittedTargets,
            PowerToughnessModCreated,
            ResolutionStart,
            ResolutionComplete,
            RevealedCardCreated,
            RevealedCardDeleted,
            ShouldntPlay,
            Shuffle,
            SyntheticEvent,
            TappedUntappedPermanent,
            TokenDeleted,
            UserActionTaken,
        };
    }
    static class Key
    {
        public const string AbilityGrpId = "abilityGrpId";
        public const string ActionType = "actionType";
        public const string AffectedIds = "affectedIds";
        public const string AffectorId = "affectorId";
        public const string Annotations = "annotations";
        public const string AuthenticateResponse = "authenticateResponse";
        public const string Category = "category";
        public const string ClientId = "clientId";
        public const string ControllerSeatId = "controllerSeatId";
        public const string Details = "details";
        public const string DiffDeletedInstanceIds = "diffDeletedInstanceIds";
        public const string GameInfo = "gameInfo";
        public const string GameObjects = "gameObjects";
        public const string GameRoomConfig = "gameRoomConfig";
        public const string GameRoomInfo = "gameRoomInfo";
        public const string GameStateId = "gameStateId";
        public const string GameStateMessage = "gameStateMessage";
        public const string GreToClientEvent = "greToClientEvent";
        public const string GreToClientMessages = "greToClientMessages";
        public const string GrpId = "grpId";
        public const string Id = "id";
        public const string InstanceId = "instanceId";
        public const string LifeTotal = "lifeTotal";
        public const string MatchGameRoomStateChangedEvent = "matchGameRoomStateChangedEvent";
        public const string MsgId = "msgId";
        public const string Name = "name";
        public const string NewId = "new_id";
        public const string ObjectInstanceIds = "objectInstanceIds";
        public const string OrigId = "orig_id";
        public const string OwnerSeatId = "ownerSeatId";
        public const string Players = "players";
        public const string ReservedPlayers = "reservedPlayers";
        public const string Stage = "stage";
        public const string Status = "status";
        public const string SystemSeatId = "systemSeatId";
        public const string TransactionId = "transactionId";
        public const string Type = "type";
        public const string UserId = "userId";
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
        public const int PlayerId = 0;
        public const int TeamId = 0;
        public const int SeatId = 0;
        public const int Id = 0;
    }
    public static class ZoneTransferCategory
    {
        // Active
        public const string CastSpell = "CastSpell";
        public const string Conjure = "Conjure";
        public const string Discard = "Discard";
        public const string Draw = "Draw";
        public const string Exile = "Exile";
        public const string Mill = "Mill";
        public const string PlayLand = "PlayLand";
        public const string Sacrifice = "Sacrifice";
        public const string Surveil = "Surveil";
        public const string Resolve = "Resolve";
        public const string Put = "Put";
        public const string Return = "Return";
        public const string Warp = "Warp";
        // Passive
        public const string SBA_Damage = "SBA_Damage";
        public const string SBA_Deathtouch = "SBA_Deathtouch";
        public const string SBA_ZeroLoyalty = "SBA_ZeroLoyalty";
        public const string SBA_ZeroToughness = "SBA_ZeroToughness";
        public const string SBA_UnattachedAura = "SBA_UnattachedAura";
        public const string Destroy = "Destroy";
        public const string Nil = "nil";   // Fizzった
        public static Boolean IsActive(string category)
        {
            return category == CastSpell ||
                   category == Conjure ||
                   category == Discard ||
                   category == Draw ||
                   category == Exile ||
                   category == Mill ||
                   category == PlayLand ||
                   category == Put ||
                   category == Sacrifice ||
                   category == Resolve ||
                   category == Return;
        }
}
    public static class PlayerWho
    {
        public const string You = "You";
        public const string Opponent = "Opponent";
        public const string Unknown = "Unknown_Player";
    }
    class Player
    {
        public int Life { get; set; }
        public int SystemSeatNumber { get; set; }
        public string Status { get; set; }
        public int MaxHandSize { get; set; }
        public int TeamId { get; set; }
        public List<int> TimerIds { get; set; }
        public int ControllerSeatId { get; set; }
        public string ControllerType { get; set; }
        public int StartingLifeTotal { get; set; }
        public class Key
        {
            public const string LifeTotal = "lifeTotal";
            public const string SystemSeatNumber = "systemSeatNumber";
            public const string Status = "status";
            public const string MaxHandSize = "maxHandSize";
            public const string TeamId = "teamId";
            public const string TimerIds = "timerIds";
            public const string ControllerSeatId = "controllerSeatId";
            public const string ControllerType = "controllerType";
            public const string StartingLifeTotal = "startingLifeTotal";
        }
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
            int ownerSeatId = Unknown.SeatId,
            int controllerSeatId = Unknown.SeatId)
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
        public string Phase { get; set; }
        public string Step { get; set; }
        public int TurnNumber { get; set; }
        public int ActivePlayer { get; set; }
        public int PriorityPlayer { get; set; }
        public int DecisionPlayer { get; set; }
        public string NextPhase { get; set; }
        public static string Key = "turnInfo";
        public static string PhaseKey = "phase";
        public static string StepKey = "step";
        public static string TurnNumberKey = "turnNumber";
        public static string ActivePlayerKey = "activePlayer";
        public static string PriorityPlayerKey = "priorityPlayer";
        public static string DecisionPlayerKey = "decisionPlayer";
        public static string NextPhaseKey = "nextPhase";
        public void Reset()
        {
            Phase = "";
            Step = "";
            TurnNumber = 0;
            ActivePlayer = Unknown.PlayerId;
            PriorityPlayer = Unknown.PlayerId;
            DecisionPlayer = Unknown.PlayerId;
            NextPhase = "";
        }
    }
    internal class JsonParser
    {
        private readonly string _cardDatabaseFile;
        private Dictionary<int, GameObject> _gameObjects;
        private CardData _cardData;
        private TurnInfo _turnInfo = new TurnInfo();
        private int _msgId = 0;
        private int _gameStateId = 0;
        private string _clientId = "";
        private List<Player> _players = new List<Player>();
        private int _yourSeatId = 0;
        private int _opponentSeatId = 0;

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

            if (json[Key.AuthenticateResponse] != null)
            {
                // 自分のsystemSeatIdを取得するためにClientIdを取得
                _clientId = json[Key.AuthenticateResponse]?[Key.ClientId]?.ToString();
                Logger.Instance.Log($"{this.GetType().Name}: ClientID: \"{_clientId}\"");
            }
            if (json[Key.MatchGameRoomStateChangedEvent] != null)
            {
                // ClientIdを元に自分と対戦相手のsystemSeatIdを取得
                if (json[Key.MatchGameRoomStateChangedEvent][Key.GameRoomInfo]?[Key.GameRoomConfig]?[Key.ReservedPlayers] != null)
                {
                    foreach (var player in json[Key.MatchGameRoomStateChangedEvent][Key.GameRoomInfo][Key.GameRoomConfig][Key.ReservedPlayers])
                    {
                        if (player[Key.UserId].ToString() == _clientId)
                        {
                            _yourSeatId = (int)player[Key.SystemSeatId];
                            Logger.Instance.Log($"{this.GetType().Name}: YourSeadId: {_yourSeatId}");
                        }
                        else
                        {
                            _opponentSeatId = (int)player[Key.SystemSeatId];
                            Logger.Instance.Log($"{this.GetType().Name}: OpponentSeadId: {_opponentSeatId}");
                        }
                    }
                }
            }
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
            _msgId = (int)(message[Key.MsgId] ?? 0);
            _gameStateId = (int)(message[Key.GameStateId] ?? 0);
            string greMessagetype = message[Key.Type]?.ToString() ?? "";
            Logger.Instance.Log($"{this.GetType().Name}: GREメッセージタイプ: {greMessagetype}, MsgId: {_msgId}, GameStateId: {_gameStateId}", LogLevel.Debug);
            if (string.IsNullOrEmpty(greMessagetype)) return;
            if (GreMessageType.IgnoreType.Contains(greMessagetype))
            {
                // 無視するメッセージタイプはログに記録して終了
                Logger.Instance.Log($"{this.GetType().Name}: 無視するGREメッセージタイプ: {greMessagetype}", LogLevel.Debug);
                return;
            }
            switch (message[Key.Type]?.ToString())
            {
                case GreMessageType.ConnectResp:
                    // ゲーム開始連絡はprocessGameInfo()でやるからここでは何もしない
                    break;
                case GreMessageType.MulliganReq:
                    Logger.Instance.Log($"{this.GetType().Name}: GREMessageType_MulliganReq");
                    EventQueue.Queue.Enqueue($"{PlayerWho.You} {GreMessageType.MulliganReq}");
                    break;
                case GreMessageType.GameStateMessage:
                case GreMessageType.QueuedGameStateMessage:
                    _gameStateId = (int)message[Key.GameStateId];
                    var gameStateMessage = message[Key.GameStateMessage];
                    string gameStateType = gameStateMessage?[Key.Type]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(gameStateType)) return;
                    switch (gameStateMessage[Key.Type]?.ToString())
                    {
                        case GameStateType.Full:
                            ProcesseGameStateTypeFullMessage(gameStateMessage);
                            break;
                        case GameStateType.Diff:
                            ProcesseGameStateTypeDiffMessage(gameStateMessage);
                            break;
                        default:
                            Logger.Instance.Log($"{this.GetType().Name}: 未知のGameStateType: {gameStateMessage[Key.Type]?.ToString()}", LogLevel.Debug);
                            break;
                    }
                    break;
                default:
                    Logger.Instance.Log($"{this.GetType().Name}: 未知のGREメッセージタイプ: {greMessagetype}", LogLevel.Debug);
                    break;
            }
        }
        private void ProcesseGameStateTypeFullMessage(JToken message)
        {
            // ゲーム状態の初期化
            _gameObjects.Clear();
            _turnInfo.Reset();
            _players.Clear();

            // turnInfoの処理（TurnInfoの更新、ターンの開始を判定）
            ProcessTurnInfo(message[TurnInfo.Key]);

            // gameInfoの処理
            ProcessGameInfo(message[Key.GameInfo]);

            // playersの処理
            ProcessPlayers(message[Key.Players]);

            // Zonesの処理（gameObjectsにoiidを登録）
            ProcessZones(message[Key.Zones]?.ToArray() ?? Array.Empty<JToken>());
        }
        private void ProcesseGameStateTypeDiffMessage(JToken message)
        {
            // turnInfoの処理（TurnInfoの更新、ターンの開始を判定）
            ProcessTurnInfo(message[TurnInfo.Key]);

            // Zonesの処理（gameObjectsにoiidを登録）
            ProcessZones(message[Key.Zones]?.ToArray() ?? Array.Empty<JToken>());

            // gameObjectsの処理（gameObjectsにoiidを登録、更新）
            ProcessGameObjects(message[Key.GameObjects]?.ToArray() ?? Array.Empty<JToken>());

            // annotationsの処理
            ProcessAnnotations(message[Key.Annotations]?.ToArray() ?? Array.Empty<JToken>());

            // gameInfoの処理
            ProcessGameInfo(message[Key.GameInfo]);
        }

        private void ProcessGameInfo(JToken gameInfo)
        {
            if (gameInfo is null) return;

            // ゲーム開始連絡
            if (gameInfo[Key.Stage]?.ToString() == GameStage.Start)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ゲーム開始");
                EventQueue.Queue.Enqueue($"{PlayerWho.You} {GameStage.Start}");
            }

            // ゲーム終了連絡
            if (gameInfo[Key.Stage]?.ToString() == GameStage.GameOver &&
                gameInfo[MatchState.Key]?.ToString() == MatchState.GameComplete)
            {
                Logger.Instance.Log($"{this.GetType().Name}: ゲーム終了");
                EventQueue.Queue.Enqueue($"{PlayerWho.You} {GameStage.GameOver}");
            }
        }
        private void ProcessPlayers(JToken players)
        {
            if (players is null) return;

            foreach (var player in players)
            {
                Player newPlayer = new Player
                {
                    Life = (int)(player[Player.Key.LifeTotal] ?? 20), // デフォルトライフは20
                    SystemSeatNumber = (int)(player[Player.Key.SystemSeatNumber] ?? Unknown.PlayerId),
                    Status = player[Player.Key.Status]?.ToString() ?? PlayerStatus.InGame,
                    MaxHandSize = (int)(player[Player.Key.MaxHandSize] ?? 7), // デフォルトの最大手札枚数は7
                    TeamId = (int)(player[Player.Key.TeamId] ?? Unknown.TeamId), 
                    TimerIds = player[Player.Key.TimerIds]?.ToObject<List<int>>() ?? new List<int>(),
                    ControllerSeatId = (int)(player[Player.Key.ControllerSeatId] ?? Unknown.SeatId),
                    ControllerType = player[Player.Key.ControllerType]?.ToString() ?? ControllerType.Player,
                    StartingLifeTotal = (int)(player[Player.Key.StartingLifeTotal] ?? 20) // デフォルトの開始ライフは20
                };
                _players.Add(newPlayer);
                Logger.Instance.Log($"{this.GetType().Name}: プレイヤー情報 - SystemSeatNumber: {newPlayer.SystemSeatNumber}, Life: {newPlayer.Life}, Status: {newPlayer.Status}, MaxHandSize: {newPlayer.MaxHandSize}, TeamId: {newPlayer.TeamId}, ControllerSeatId: {newPlayer.ControllerSeatId}, ControllerType: {newPlayer.ControllerType}, StartingLifeTotal: {newPlayer.StartingLifeTotal}", LogLevel.Debug);
            }
        }
        private void ProcessAnnotations(IList<JToken> annotations)
        {
            if (annotations is null) return;

            foreach (var annotation in annotations)
            {
                int id = (int)(annotation[Key.Id] ?? Unknown.Id);
                int affectorId = (int)(annotation[Key.AffectorId] ?? Unknown.Id);
                int[] affectedIds = annotation[Key.AffectedIds]?.ToObject<int[]>() ?? Array.Empty<int>();
                string playerWho;
                string annotationType = annotation[Key.Type]?[0]?.ToString() ?? "";
                if (string.IsNullOrEmpty(annotationType)) continue;
                if (AnnotationType.Ignore.Contains(annotationType))
                {
                    Logger.Instance.Log($"{this.GetType().Name}: 無視するアノテーションタイプ: {annotationType}", LogLevel.Debug);
                    continue;
                }
                switch (annotationType)
                {
                    case AnnotationType.ObjectIdChanged
                        when annotation[Key.Details] != null:
                        {
                            // オブジェクトIDの変更
                            int origId = (int)annotation[Key.Details][0][Key.ValueInt32][0];
                            int newId = (int)annotation[Key.Details][1][Key.ValueInt32][0];

                            // ProcessZoneと競合するので、newIdが存在しない場合だけGameObjectを生成する
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
                            break;
                        }
                    case AnnotationType.TokenCreated:
                        {
                            foreach (int affectedId in affectedIds)
                            {
                                // ProcessZoneと競合するので、newIdが存在しない場合だけGameObjectを生成する
                                if (!_gameObjects.ContainsKey(affectedId))
                                {
                                    _gameObjects[affectedId] = new GameObject(
                                        _gameObjects[affectedId].GrpId,
                                        _gameObjects[affectedId].Name,
                                        _gameObjects[affectedId].Type,
                                        _gameObjects[affectedId].ZoneId,
                                        _gameObjects[affectedId].Visible,
                                        _gameObjects[affectedId].OwnerSeatId,
                                        _gameObjects[affectedId].ControllerSeatId);
                                }
                                Logger.Instance.Log($"{this.GetType().Name}: トークン生成 - id: {affectedId}, {_gameObjects[affectedId].GetDescription()}", LogLevel.Debug);
                                EventQueue.Queue.Enqueue($"{GetPlayerWho(_gameObjects[affectedId].ControllerSeatId)} {annotationType} \"{_gameObjects[affectedId].Name}\"");
                            }
                            break;
                        }
                    case AnnotationType.NewTurnStarted:
                        Logger.Instance.Log($"{this.GetType().Name}: {GetPlayerWho(affectorId)} のターン開始: {_turnInfo.TurnNumber}", LogLevel.Debug);
                        EventQueue.Queue.Enqueue($"{GetPlayerWho(affectorId)} {AnnotationType.NewTurnStarted} {_turnInfo.TurnNumber}");
                        break;
                    case AnnotationType.ModifiedLife:
                        int lifeDiff = (int)(annotation[Key.Details][0][Key.ValueInt32][0] ?? 0);
                        if (lifeDiff == 0) break;
                        foreach (int affectedId in affectedIds)
                        {
                            var player = GetPlayer(affectedId);
                            if (player == null) continue;
                            Logger.Instance.Log($"{this.GetType().Name}: {GetPlayerWho(affectedId)} のライフ変動: {player.Life} -> {player.Life + lifeDiff}", LogLevel.Debug);
                            player.Life += lifeDiff;
                            EventQueue.Queue.Enqueue($"{GetPlayerWho(affectedId)} {AnnotationType.ModifiedLife} {lifeDiff} {player.Life}");
                        }
                        break;
                    case AnnotationType.ZoneTransfer
                        when annotation[Key.Details] != null:
                        {
                            // ゾーンの移動
                            int zoneSrcId = (int)(annotation[Key.Details][0]?[Key.ValueInt32]?[0] ?? Unknown.ZoneId);
                            int zoneDestId = (int)(annotation[Key.Details][1]?[Key.ValueInt32]?[0] ?? Unknown.ZoneId);
                            string zoneTransferCategory = annotation[Key.Details]?[2]?[Key.ValueString]?[0].ToString();
                            if (affectorId == Unknown.Id)
                            {
                                foreach (int seadId in new List<int> { 1, 2 })
                                {
                                    if (ZoneId.PlayerZones[seadId].Contains(zoneSrcId) || ZoneId.PlayerZones[seadId].Contains(zoneDestId))
                                    {
                                        affectorId = seadId;
                                    }
                                }
                            }
                            // 【改善可能】affectorIdが10未満ならプレイヤー、そうでないならGameObjectのcontrollerかownerとする。
                            // 10は仮。いずれプレイヤー数が増える可能性があるので。
                            foreach (int affectedId in affectedIds)
                            {
                                // コントローラ > オーナー > ゾーンのアクター の順でプレイヤーを決定
                                playerWho = affectedId < 10 ? GetPlayerWho(affectedId) :
                                (_gameObjects[affectedId].ControllerSeatId != Unknown.SeatId ? GetPlayerWho(_gameObjects[affectedId].ControllerSeatId) :
                                    (_gameObjects[affectedId].OwnerSeatId != Unknown.SeatId ? GetPlayerWho(_gameObjects[affectedId].OwnerSeatId) : Unknown.Player)
                                );
                                if (playerWho == Unknown.Player)
                                {
                                    int actorSeatId = GetSeatIdByZoneId(zoneSrcId, zoneDestId);
                                    playerWho = GetPlayerWho(actorSeatId);
                                }
                                /*
                                if (_gameObjects.ContainsKey(affectedId))
                                {
                                    _gameObjects[affectedId].ZoneId = zoneDestId;
                                }
                                */
                                Logger.Instance.Log($"{this.GetType().Name}: {playerWho} {zoneTransferCategory} \"{_gameObjects[affectedId].Name}\" {zoneSrcId} {zoneDestId}", LogLevel.Debug);
                                EventQueue.Queue.Enqueue($"{playerWho} {zoneTransferCategory} \"{_gameObjects[affectedId].Name}\" {zoneSrcId} {zoneDestId}");
                            }
                            break;
                        }
                    default:
                        Logger.Instance.Log($"{this.GetType().Name}: 未知のアノテーションタイプ: {annotationType}", LogLevel.Debug);
                        break;
                }
            }
        }

        private void ProcessTurnInfo(JToken turnInfo)
        {
            if (turnInfo != null)
            {
                var newPhase = turnInfo[TurnInfo.PhaseKey]?.ToString() ?? _turnInfo.Phase;
                var newStep = turnInfo[TurnInfo.StepKey]?.ToString() ?? _turnInfo.Step;
                var newTurnNumber = (int)(turnInfo[TurnInfo.TurnNumberKey] ?? _turnInfo.TurnNumber);
                var newActivePlayer = (int)(turnInfo[TurnInfo.ActivePlayerKey] ?? _turnInfo.ActivePlayer);
                var newPriorityPlayer = (int)(turnInfo[TurnInfo.PriorityPlayerKey] ?? _turnInfo.PriorityPlayer);
                var newDecisionPlayer = (int)(turnInfo[TurnInfo.DecisionPlayerKey] ?? _turnInfo.DecisionPlayer);
                var newNextPhase = turnInfo[TurnInfo.NextPhaseKey]?.ToString() ?? _turnInfo.NextPhase;
                // 先手第１ターンだけここで実況（本当はターンやフェーズ関連の実況は全部ここに集約すべきだと思う）
                if (_turnInfo.TurnNumber == 0 && newTurnNumber == 1)
                {
                    Logger.Instance.Log($"{this.GetType().Name}: {GetPlayerWho(newActivePlayer)} のターン開始: {newTurnNumber}", LogLevel.Debug);
                    EventQueue.Queue.Enqueue($"{GetPlayerWho(newActivePlayer)} {AnnotationType.NewTurnStarted} {newTurnNumber}");
                }

                // 更新
                _turnInfo.Phase = turnInfo[TurnInfo.PhaseKey]?.ToString() ?? _turnInfo.Phase;
                _turnInfo.Step = turnInfo[TurnInfo.StepKey]?.ToString() ?? _turnInfo.Step;
                _turnInfo.TurnNumber = (int)(turnInfo[TurnInfo.TurnNumberKey] ?? _turnInfo.TurnNumber);
                _turnInfo.ActivePlayer = (int)(turnInfo[TurnInfo.ActivePlayerKey] ?? _turnInfo.ActivePlayer);
                _turnInfo.PriorityPlayer = (int)(turnInfo[TurnInfo.PriorityPlayerKey] ?? _turnInfo.PriorityPlayer);
                _turnInfo.DecisionPlayer = (int)(turnInfo[TurnInfo.DecisionPlayerKey] ?? _turnInfo.DecisionPlayer);
                _turnInfo.NextPhase = turnInfo[TurnInfo.NextPhaseKey]?.ToString() ?? _turnInfo.NextPhase;

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

        private void ProcessZones(IList<JToken> zones)
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
                        if (_gameObjects[oiid].ZoneId != (int)zone[Key.ZoneId])
                        {
                            // オブジェクトのゾーンが異なる場合は更新
                            Logger.Instance.Log($"{this.GetType().Name}: オブジェクトゾーン更新 - InstanceId: {oiid}, OldZoneId: {_gameObjects[oiid].ZoneId}, NewZoneId: {(int)zone[Key.ZoneId]}, {_gameObjects[oiid].GetDescription()}", LogLevel.Debug);
                            _gameObjects[oiid].ZoneId = (int)zone[Key.ZoneId];
                        }
                        // ゾーンに合わせてownerSeatIdを変更
                        int seatId = GetSeatIdByZoneId(_gameObjects[oiid].ZoneId);
                        if (seatId != Unknown.SeatId && _gameObjects[oiid].OwnerSeatId != seatId && _gameObjects[oiid].OwnerSeatId == Unknown.SeatId)
                        {
                            Logger.Instance.Log($"{this.GetType().Name}: オブジェクトオーナー更新 - InstanceId: {oiid}, OldOwnerSeatId: {_gameObjects[oiid].OwnerSeatId}, NewOwnerSeatId: {seatId}, {_gameObjects[oiid].GetDescription()}", LogLevel.Debug);
                            _gameObjects[oiid].OwnerSeatId = seatId;
                        }
                        _gameObjects[oiid].OwnerSeatId = (int)(zone[Key.OwnerSeatId] ?? _gameObjects[oiid].OwnerSeatId);
                    }
                }
            }
        }

        private void ProcessGameObjects(IList<JToken> gameObjects)
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
        private Player GetPlayer(int seatId)
        {
            if (seatId <= 0 || seatId > _players.Count)
            {
                return null; // Unknown Player
            }
            foreach (var player in _players)
            {
                if (player.SystemSeatNumber == seatId)
                {
                    return player;
                }
            }
            return null;
        }
        public string GetPlayerWho(int seatId)
        {
            if (seatId == _yourSeatId)
            {
                return PlayerWho.You;
            }
            else if (seatId == _opponentSeatId)
            {
                return PlayerWho.Opponent;
            }
            else
            {
                return PlayerWho.Unknown;
            }
        }
        private static int GetSeatIdByZoneId(int fromZoneId, int toZoneId = Unknown.ZoneId)
        {
            foreach (var player in ZoneId.PlayerZones)
            {
                if (player.Value.Contains(fromZoneId) || player.Value.Contains(toZoneId))
                {
                    return player.Key;
                }
            }
            return Unknown.SeatId;
        }

    }
}