using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    [Serializable]
    public class SaveData
    {
        public int SaveVersion = 1;
        public string CurrentMap = MapId.Map1_Kitchen.ToString();
        public string CurrentMapState = MapState.Active.ToString();

        public bool M1_MiniDone = false;
        public bool M2_MiniDone = false;
        public bool M3_MiniDone = false;

        public bool PedestalPlaced = false;
        public bool BridgeReady = false;
        public bool ReunionSeen = false;
        public bool IntroSeen = true;
        public bool GameCompleted = false;

        public List<string> InventoryItems = new List<string>();
        public List<string> SolvedQuizzes = new List<string>();

        // Serialization helper for Dictionary
        public List<string> SlotKeys = new List<string>();
        public List<string> SlotValues = new List<string>();
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        private string saveFilePath;
        private bool isSaving = false;

        public event Action OnGameSaved;
        public event Action OnGameLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        }

        private void Start()
        {
            // Tự động tải dữ liệu nếu có file save
            if (HasSaveData())
            {
                LoadGame();
            }
        }

        public bool HasSaveData()
        {
            if (string.IsNullOrEmpty(saveFilePath))
            {
                saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            }
            return File.Exists(saveFilePath);
        }

        public void RequestAutoSave()
        {
            if (isSaving) return;
            SaveGame();
        }

        public void SaveGame()
        {
            try
            {
                isSaving = true;
                var data = new SaveData();

                if (GameStateManager.Instance != null)
                {
                    data.CurrentMap = GameStateManager.Instance.CurrentMap.ToString();
                    data.CurrentMapState = GameStateManager.Instance.CurrentMapState.ToString();
                    data.M1_MiniDone = GameStateManager.Instance.M1_MiniDone;
                    data.M2_MiniDone = GameStateManager.Instance.M2_MiniDone;
                    data.M3_MiniDone = GameStateManager.Instance.M3_MiniDone;
                    data.PedestalPlaced = GameStateManager.Instance.PedestalPlaced;
                    data.BridgeReady = GameStateManager.Instance.BridgeReady;
                    data.ReunionSeen = GameStateManager.Instance.ReunionSeen;
                    data.IntroSeen = GameStateManager.Instance.IntroSeen;
                    data.GameCompleted = GameStateManager.Instance.GameCompleted;
                }

                if (InventoryManager.Instance != null)
                {
                    data.InventoryItems = new List<string>(InventoryManager.Instance.Items);
                }

                if (QuizManager.Instance != null)
                {
                    data.SolvedQuizzes = new List<string>(QuizManager.Instance.SolvedQuizIds);
                }

                if (PlacementManager.Instance != null)
                {
                    foreach (var kvp in PlacementManager.Instance.FilledSlots)
                    {
                        data.SlotKeys.Add(kvp.Key);
                        data.SlotValues.Add(kvp.Value);
                    }
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[SaveLoadManager] Đã lưu tiến trình game xuống: {saveFilePath}");

                OnGameSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi khi lưu tiến trình: {ex.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }

        public bool LoadGame()
        {
            if (!HasSaveData())
            {
                Debug.Log("[SaveLoadManager] Không tìm thấy file save để tải.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogError("[SaveLoadManager] Dữ liệu file save bị hỏng.");
                    return false;
                }

                // Nạp vào Inventory
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.LoadItems(data.InventoryItems);
                }

                // Nạp vào Quiz
                if (QuizManager.Instance != null)
                {
                    QuizManager.Instance.LoadSolvedQuizzes(data.SolvedQuizzes);
                }

                // Nạp vào Placement
                if (PlacementManager.Instance != null)
                {
                    var slots = new Dictionary<string, string>();
                    for (int i = 0; i < data.SlotKeys.Count && i < data.SlotValues.Count; i++)
                    {
                        slots[data.SlotKeys[i]] = data.SlotValues[i];
                    }
                    PlacementManager.Instance.LoadFilledSlots(slots);
                }

                // Nạp vào GameStateManager
                if (GameStateManager.Instance != null)
                {
                    if (Enum.TryParse<MapId>(data.CurrentMap, out var mapId) &&
                        Enum.TryParse<MapState>(data.CurrentMapState, out var mapState))
                    {
                        GameStateManager.Instance.LoadState(
                            mapId,
                            mapState,
                            data.M1_MiniDone,
                            data.M2_MiniDone,
                            data.M3_MiniDone,
                            data.PedestalPlaced,
                            data.BridgeReady,
                            data.ReunionSeen,
                            data.GameCompleted
                        );
                    }
                }

                Debug.Log("[SaveLoadManager] Khôi phục tiến trình thành công từ file save!");
                OnGameLoaded?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi khi nạp dữ liệu save: {ex.Message}");
                return false;
            }
        }

        public void DeleteSave()
        {
            if (HasSaveData())
            {
                File.Delete(saveFilePath);
                Debug.Log("[SaveLoadManager] Đã xóa file save tiến trình.");
            }
        }

        /// <summary>
        /// Kiểm tra chống nhân đôi vật phẩm: Nếu vật phẩm đã có trong túi hoặc đã đặt lên slot
        /// </summary>
        public bool IsItemAlreadyCollectedOrPlaced(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(itemId))
            {
                return true;
            }

            if (PlacementManager.Instance != null)
            {
                foreach (var placedItem in PlacementManager.Instance.FilledSlots.Values)
                {
                    if (placedItem == itemId) return true;
                }
            }

            return false;
        }
    }
}
