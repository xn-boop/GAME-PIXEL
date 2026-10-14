using System;
using UnityEngine;
using JourneyOfMemories.Core.Data;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        [SerializeField] private List<int> collectedMapFragments = new List<int>();
        [SerializeField] private List<int> placedMapFragments = new List<int>();

        [Header("Game Flags")]
        [SerializeField] private bool introSeen = true;
        [SerializeField] private bool finalPageConfirmed = false;
        [SerializeField] private bool gameCompleted = false;
        private readonly HashSet<string> controlLocks = new HashSet<string>();

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
        public bool FinalPageConfirmed => finalPageConfirmed;
        public IReadOnlyList<int> CollectedMapFragments => collectedMapFragments.AsReadOnly();
        public IReadOnlyList<int> PlacedMapFragments => placedMapFragments.AsReadOnly();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ApplyPlayerControlLock(); }

        private void Start()
        {
            // Kiểm tra trạng thái lúc khởi động
            CheckObjectives();
        }

        private void SetMapState(MapState newState)
        {
            if (currentMapState == newState) return;
            currentMapState = newState;
            Debug.Log($"[GameStateManager] Chuyển trạng thái Map: {newState}");
            OnMapStateChanged?.Invoke(newState);

            if (newState == MapState.MemoryPending || newState == MapState.JournalPending)
            {
                AcquirePlayerControlLock("MapFlow");
            }
            else if (newState == MapState.Completed || newState == MapState.Active)
            {
                ReleasePlayerControlLock("MapFlow");
            }
        }

        public bool SetMiniGameDone(int mapIndex, bool isDone)
        {
            if (mapIndex < 1 || mapIndex > 3) return false;
            if (mapIndex == 3 && isDone && placedMapFragments.Count != 4) return false;
            switch (mapIndex)
            {
                case 1: m1_miniDone = isDone; break;
                case 2: m2_miniDone = isDone; break;
                case 3: m3_miniDone = isDone; break;
            }
            Debug.Log($"[GameStateManager] MiniGame Map {mapIndex} cập nhật: {isDone}");
            CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool CollectMapFragment(int fragmentIndex)
        {
            if (currentMap != MapId.Map3_Garden || fragmentIndex < 1 || fragmentIndex > 4)
                return false;
            if (collectedMapFragments.Contains(fragmentIndex)) return true;
            collectedMapFragments.Add(fragmentIndex);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool PlaceMapFragment(int fragmentIndex)
        {
            if (!collectedMapFragments.Contains(fragmentIndex)) return false;
            if (!placedMapFragments.Contains(fragmentIndex)) placedMapFragments.Add(fragmentIndex);
            if (placedMapFragments.Count == 4) m3_miniDone = true;
            CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool MarkPedestalPlaced()
        {
            if (currentMap != MapId.Map3_Garden || !m3_miniDone ||
                PlacementManager.Instance == null || !PlacementManager.Instance.IsSlotFilled(GameIds.Slot.Pedestal))
                return false;
            pedestalPlaced = true;
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool MarkBridgeReady()
        {
            if (!pedestalPlaced) return false;
            bridgeReady = true;
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool MarkReunionSeen()
        {
            if (currentMap != MapId.Map3_Garden || !bridgeReady) return false;
            reunionSeen = true;
            CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool ConfirmFinalPage()
        {
            if (currentMap != MapId.Ending || currentMapState != MapState.Active) return false;
            finalPageConfirmed = true;
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool CompleteGame()
        {
            if (currentMap != MapId.Ending || !finalPageConfirmed || gameCompleted) return false;
            gameCompleted = true;
            SetMapState(MapState.Completed);
            OnMapCompleted?.Invoke(MapId.Ending);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
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

        public bool NotifyMemoryFinished()
        {
            if (currentMapState != MapState.MemoryPending) return false;
            SetMapState(MapState.JournalPending);
            OnJournalReady?.Invoke(currentMap);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        /// <summary>
        /// API Hợp đồng: ConfirmJournal
        /// Khi người chơi ấn "Ghi nhớ" ở trang nhật ký
        /// </summary>
        public bool ConfirmJournal(MapId mapId)
        {
            if (mapId != currentMap || currentMapState != MapState.JournalPending)
            {
                Debug.LogWarning($"[GameStateManager] Từ chối xác nhận nhật ký {mapId} ở state {currentMapState}.");
                return false;
            }

            SetMapState(MapState.Completed);
            Debug.Log($"[GameStateManager] Xác nhận nhật ký cho {mapId}. Map đã hoàn tất!");
            OnMapCompleted?.Invoke(mapId);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
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

            if (targetMapId == MapId.Ending && !reunionSeen) return false;

            currentMap = targetMapId;
            SetMapState(MapState.Active);
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public void SetPlayerControlLock(bool locked)
        {
            if (locked) AcquirePlayerControlLock("Legacy");
            else ReleasePlayerControlLock("Legacy");
        }

        public void AcquirePlayerControlLock(string owner)
        {
            if (string.IsNullOrEmpty(owner)) return;
            controlLocks.Add(owner);
            ApplyPlayerControlLock();
        }

        public void ReleasePlayerControlLock(string owner)
        {
            if (string.IsNullOrEmpty(owner)) return;
            controlLocks.Remove(owner);
            ApplyPlayerControlLock();
        }

        private void ApplyPlayerControlLock()
        {
            bool locked = controlLocks.Count > 0;
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
            bool introWasSeen,
            bool finalWasConfirmed,
            bool doneGame,
            IEnumerable<int> collectedFragments = null,
            IEnumerable<int> placedFragments = null)
        {
            currentMap = map;
            currentMapState = state;
            m1_miniDone = m1Done;
            m2_miniDone = m2Done;
            m3_miniDone = m3Done;
            pedestalPlaced = pedPlaced;
            bridgeReady = brReady;
            reunionSeen = reunSeen;
            introSeen = introWasSeen;
            finalPageConfirmed = finalWasConfirmed;
            gameCompleted = doneGame;
            collectedMapFragments = collectedFragments != null ? new List<int>(collectedFragments) : new List<int>();
            placedMapFragments = placedFragments != null ? new List<int>(placedFragments) : new List<int>();

            controlLocks.Clear();
            if (currentMapState == MapState.MemoryPending || currentMapState == MapState.JournalPending)
                controlLocks.Add("MapFlow");
            OnMapStateChanged?.Invoke(currentMapState);
            ApplyPlayerControlLock();
        }

        public void ResetProgress()
        {
            currentMap = MapId.Map1_Kitchen;
            currentMapState = MapState.Active;
            m1_miniDone = m2_miniDone = m3_miniDone = false;
            pedestalPlaced = bridgeReady = reunionSeen = false;
            introSeen = false;
            finalPageConfirmed = false;
            gameCompleted = false;
            collectedMapFragments.Clear();
            placedMapFragments.Clear();
            controlLocks.Clear();
            OnMapStateChanged?.Invoke(currentMapState);
            ApplyPlayerControlLock();
        }
    }
}
