using System;
using UnityEngine;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("State Information")]
        [SerializeField] private MapId currentMap = MapId.Map1_Kitchen;
        [SerializeField] private MapState currentMapState = MapState.Active;

        [Header("Mini-Game Flags")]
        [SerializeField] private bool m1_miniDone = false; // Mèo vuốt ve
        [SerializeField] private bool m2_miniDone = false; // Lau ảnh gia đình
        [SerializeField] private bool m3_miniDone = false; // Ghép bản đồ

        [Header("Map 3 Special Flags")]
        [SerializeField] private bool pedestalPlaced = false;
        [SerializeField] private bool bridgeReady = false;
        [SerializeField] private bool reunionSeen = false;

        [Header("Game Flags")]
        [SerializeField] private bool introSeen = true;
        [SerializeField] private bool finalPageConfirmed = false;
        [SerializeField] private bool gameCompleted = false;

        // Events
        public event Action<MapState> OnMapStateChanged;
        public event Action<MapId> OnMemoryTriggered;
        public event Action<MapId> OnJournalReady;
        public event Action<MapId> OnMapCompleted;
        public event Action<bool> OnPlayerControlLockChanged; // true = locked

        public MapId CurrentMap => currentMap;
        public MapState CurrentMapState => currentMapState;
        public bool M1_MiniDone => m1_miniDone;
        public bool M2_MiniDone => m2_miniDone;
        public bool M3_MiniDone => m3_miniDone;
        public bool PedestalPlaced => pedestalPlaced;
        public bool BridgeReady => bridgeReady;
        public bool ReunionSeen => reunionSeen;
        public bool IntroSeen => introSeen;
        public bool GameCompleted => gameCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Kiểm tra trạng thái lúc khởi động
            CheckObjectives();
        }

        public void SetMapState(MapState newState)
        {
            if (currentMapState == newState) return;
            currentMapState = newState;
            Debug.Log($"[GameStateManager] Chuyển trạng thái Map: {newState}");
            OnMapStateChanged?.Invoke(newState);

            if (newState == MapState.MemoryPending || newState == MapState.JournalPending)
            {
                SetPlayerControlLock(true);
            }
            else if (newState == MapState.Completed || newState == MapState.Active)
            {
                SetPlayerControlLock(false);
            }
        }

        public void SetMiniGameDone(int mapIndex, bool isDone)
        {
            switch (mapIndex)
            {
                case 1: m1_miniDone = isDone; break;
                case 2: m2_miniDone = isDone; break;
                case 3: m3_miniDone = isDone; break;
            }
            Debug.Log($"[GameStateManager] MiniGame Map {mapIndex} cập nhật: {isDone}");
            CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();
        }

        public void CheckObjectives()
        {
            if (currentMapState != MapState.Active) return;

            switch (currentMap)
            {
                case MapId.Map1_Kitchen:
                    CheckMap1Objectives();
                    break;
                case MapId.Map2_DressingRoom:
                    CheckMap2Objectives();
                    break;
                case MapId.Map3_Garden:
                    CheckMap3Objectives();
                    break;
            }
        }

        private void CheckMap1Objectives()
        {
            // Cổng logic Map 1: MiniGame Mèo == Done + Q01-Q03 == Solved + 3 slots mâm cơm == Filled
            bool catDone = m1_miniDone;
            bool q01 = QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved("Q01");
            bool q02 = QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved("Q02");
            bool q03 = QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved("Q03");

            bool slotThitKho = PlacementManager.Instance != null && PlacementManager.Instance.IsSlotFilled("Slot_ThitKhoTrung");
            bool slotKimchi = PlacementManager.Instance != null && PlacementManager.Instance.IsSlotFilled("Slot_Kimchi");
            bool slotComCuon = PlacementManager.Instance != null && PlacementManager.Instance.IsSlotFilled("Slot_ComCuon");

            if (catDone && q01 && q02 && q03 && slotThitKho && slotKimchi && slotComCuon)
            {
                TriggerMemorySequence();
            }
        }

        private void CheckMap2Objectives()
        {
            bool photoCleaned = m2_miniDone;
            bool q04 = QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved("Q04");
            bool q05 = QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved("Q05");

            bool slotMom = PlacementManager.Instance != null && PlacementManager.Instance.IsSlotFilled("Slot_Mother");
            bool slotDad = PlacementManager.Instance != null && PlacementManager.Instance.IsSlotFilled("Slot_Father");

            if (photoCleaned && q04 && q05 && slotMom && slotDad)
            {
                TriggerMemorySequence();
            }
        }

        private void CheckMap3Objectives()
        {
            if (reunionSeen)
            {
                TriggerMemorySequence();
            }
        }

        private void TriggerMemorySequence()
        {
            if (currentMapState == MapState.MemoryPending || currentMapState == MapState.Completed) return;

            SetMapState(MapState.MemoryPending);
            Debug.Log($"[GameStateManager] Kích hoạt cảnh Ký ức cho {currentMap}!");
            OnMemoryTriggered?.Invoke(currentMap);
            SaveLoadManager.Instance?.RequestAutoSave();
        }

        public void NotifyMemoryFinished()
        {
            SetMapState(MapState.JournalPending);
            OnJournalReady?.Invoke(currentMap);
        }

        /// <summary>
        /// API Hợp đồng: ConfirmJournal
        /// Khi người chơi ấn "Ghi nhớ" ở trang nhật ký
        /// </summary>
        public void ConfirmJournal(MapId mapId)
        {
            if (mapId != currentMap) return;

            SetMapState(MapState.Completed);
            Debug.Log($"[GameStateManager] Xác nhận nhật ký cho {mapId}. Map đã hoàn tất!");
            OnMapCompleted?.Invoke(mapId);
            SaveLoadManager.Instance?.RequestAutoSave();
        }

        /// <summary>
        /// API Hợp đồng: RequestTransition
        /// </summary>
        public bool RequestTransition(MapId targetMapId)
        {
            if (currentMapState != MapState.Completed)
            {
                Debug.LogWarning($"[GameStateManager] Từ chối chuyển map: {currentMap} chưa hoàn tất (State: {currentMapState}).");
                return false;
            }

            // Kiểm tra tính tuần tự
            bool isValidNext = false;
            if (currentMap == MapId.Map1_Kitchen && targetMapId == MapId.Map2_DressingRoom) isValidNext = true;
            else if (currentMap == MapId.Map2_DressingRoom && targetMapId == MapId.Map3_Garden) isValidNext = true;
            else if (currentMap == MapId.Map3_Garden && targetMapId == MapId.Ending) isValidNext = true;

            if (!isValidNext)
            {
                Debug.LogWarning($"[GameStateManager] Không thể chuyển trực tiếp từ {currentMap} sang {targetMapId}.");
                return false;
            }

            currentMap = targetMapId;
            SetMapState(MapState.Active);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public void SetPlayerControlLock(bool locked)
        {
            OnPlayerControlLockChanged?.Invoke(locked);

            var movement = FindAnyObjectByType<PlayerMovement>();
            if (movement != null) movement.SetMovementLocked(locked);

            var interaction = FindAnyObjectByType<PlayerInteraction>();
            if (interaction != null) interaction.SetInteractionLocked(locked);
        }

        public void LoadState(
            MapId map,
            MapState state,
            bool m1Done,
            bool m2Done,
            bool m3Done,
            bool pedPlaced,
            bool brReady,
            bool reunSeen,
            bool doneGame)
        {
            currentMap = map;
            currentMapState = state;
            m1_miniDone = m1Done;
            m2_miniDone = m2Done;
            m3_miniDone = m3Done;
            pedestalPlaced = pedPlaced;
            bridgeReady = brReady;
            reunionSeen = reunSeen;
            gameCompleted = doneGame;

            OnMapStateChanged?.Invoke(currentMapState);
        }
    }
}
