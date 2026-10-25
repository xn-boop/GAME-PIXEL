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
        public int SaveVersion = 2;
        public string CurrentMap = MapId.Map1_Kitchen.ToString();
        public string CurrentMapState = MapState.Active.ToString();
        public bool M1_MiniDone = false;
        public bool M2_MiniDone = false;
        public bool M3_MiniDone = false;
        public bool PedestalPlaced = false;
        public bool BridgeReady = false;
        public bool ReunionSeen = false;
        public bool IntroSeen = true;
        public bool FinalPageConfirmed = false;
        public bool GameCompleted = false;
        public List<string> InventoryItems = new List<string>();
        public List<string> SolvedQuizzes = new List<string>();
        public List<int> CollectedMapFragments = new List<int>();
        public List<int> PlacedMapFragments = new List<int>();
        // Serialization helper for Dictionary
        public List<string> SlotKeys = new List<string>();
        public List<string> SlotValues = new List<string>();
    }

    public enum SaveLoadStatus
    {
        None,
        Success,
        NoSaveData,
        CorruptedData,
        UnsupportedVersion,
        InvalidMapData,
        LoadError,
        NoAccount
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

#if UNITY_EDITOR
        public static bool DisableDiskPersistenceForTests { get; set; }
#endif

        private string saveFilePath;
        private string temporarySaveFilePath;
        private string currentAccountId = null;
        public string CurrentAccountId => currentAccountId;
        private bool isSaving = false;
        private int batchDepth = 0;
        private bool saveRequestedDuringBatch = false;

        public event Action OnGameSaved;
        public event Action OnGameLoaded;

        public SaveLoadStatus LastLoadStatus { get; private set; } = SaveLoadStatus.None;
        public string LastErrorMessage { get; private set; } = string.Empty;

        private void Awake()
        {
#if UNITY_EDITOR
            Instance = this;
#else
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#endif
            EnsurePaths();

            // Yêu cầu 1: Không tự động LoadGame() từ savegame.json chung trong Awake() khi chưa xác định tài khoản
            if (!string.IsNullOrEmpty(currentAccountId))
            {
#if UNITY_EDITOR
                if (!DisableDiskPersistenceForTests && HasSaveData()) LoadGame();
#else
                if (HasSaveData()) LoadGame();
#endif
            }
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetCurrentAccount(string accountId)
        {
            currentAccountId = accountId;
            UpdateSavePaths();
        }

        public void EnsurePaths()
        {
            UpdateSavePaths();
        }

        private void UpdateSavePaths()
        {
            if (!string.IsNullOrEmpty(currentAccountId))
            {
                string saveDir = Path.Combine(Application.persistentDataPath, "saves");
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                string safeId = ConvertSafeId(currentAccountId);
                saveFilePath = Path.Combine(saveDir, $"save_{safeId}.json");
                temporarySaveFilePath = saveFilePath + ".tmp";

                // Kiểm tra và di chuyển an toàn savegame.json cũ sang tài khoản nếu xác định được chủ sở hữu
                TryMigrateLegacySave(currentAccountId, saveFilePath);
            }
            else
            {
                // Yêu cầu 3: Khi chưa có tài khoản, không trỏ saveFilePath về savegame.json cũ để không can thiệp hoặc ghi đè
                saveFilePath = null;
                temporarySaveFilePath = null;
            }
        }

        public string GetSaveFilePath()
        {
            EnsurePaths();
            if (!string.IsNullOrEmpty(saveFilePath)) return saveFilePath;
            // Trả về đường dẫn legacy savegame.json (chỉ đọc) nếu chưa có tài khoản
            return Path.Combine(Application.persistentDataPath, "savegame.json");
        }

        public bool HasLegacySaveData()
        {
            string legacySavePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            return File.Exists(legacySavePath);
        }

        public static string ConvertSafeId(string input)
        {
            if (string.IsNullOrEmpty(input)) return "default";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            string b64 = Convert.ToBase64String(bytes);
            return b64.Replace("/", "_").Replace("+", "-").Replace("=", "");
        }

        private void TryMigrateLegacySave(string accountId, string targetPath)
        {
            try
            {
                string legacySavePath = Path.Combine(Application.persistentDataPath, "savegame.json");
                if (!File.Exists(legacySavePath)) return;

                // Nếu tài khoản đã có file save riêng rồi thì không ghi đè
                if (File.Exists(targetPath)) return;

                // Nếu file cũ đã được đánh dấu migrated hoàn tất trước đó thì bỏ qua
                string migratedMarker = legacySavePath + ".migrated";
                if (File.Exists(migratedMarker)) return;

                // Yêu cầu 4: Xác định chủ sở hữu hợp lệ của savegame.json cũ với bằng chứng tin cậy
                // Tuyệt đối không suy luận chủ sở hữu chỉ từ việc có LoggedInUser_Email hoặc LocalAccountManager.Instance == null
                bool isOwnerIdentified = false;

                // Bằng chứng 1: Tệp marker sở hữu riêng lẻ (savegame.json.owner) chứa đúng accountId hoặc email
                string ownerMarkerPath = legacySavePath + ".owner";
                string currentEmail = null;
                if (LocalAccountManager.Instance != null)
                {
                    var acc = LocalAccountManager.Instance.FindAccountById(accountId);
                    if (acc != null) currentEmail = acc.email;
                }

                if (File.Exists(ownerMarkerPath))
                {
                    try
                    {
                        string ownerContent = File.ReadAllText(ownerMarkerPath).Trim();
                        if (string.Equals(ownerContent, accountId, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrEmpty(currentEmail) && string.Equals(ownerContent, currentEmail, StringComparison.OrdinalIgnoreCase)))
                        {
                            isOwnerIdentified = true;
                        }
                    }
                    catch { }
                }

                // Bằng chứng 2: Thiết bị chỉ có duy nhất một tài khoản cục bộ từ trước đến nay
                // và tài khoản đó chính là tài khoản hiện tại đang yêu cầu chuyển đổi
                if (!isOwnerIdentified && LocalAccountManager.Instance != null)
                {
                    var allAccounts = LocalAccountManager.Instance.AllAccounts;
                    if (allAccounts != null && allAccounts.Count == 1 && string.Equals(allAccounts[0].accountId, accountId, StringComparison.OrdinalIgnoreCase))
                    {
                        isOwnerIdentified = true;
                    }
                }

                // Nếu không có bằng chứng sở hữu đủ tin cậy: giữ nguyên save cũ và thông báo
                if (!isOwnerIdentified)
                {
                    Debug.LogWarning("[SaveLoadManager] Phát hiện file save cũ (savegame.json) nhưng chưa xác định được chủ sở hữu đủ tin cậy. Giữ nguyên file cũ để xử lý an toàn; không tự gán hoặc xóa.");
                    return;
                }

                // Yêu cầu 5: Tạo backup an toàn trước khi chuyển đổi
                string backupDir = Path.Combine(Application.persistentDataPath, "saves", "backups");
                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                string backupFile = Path.Combine(backupDir, $"savegame_legacy_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
                File.Copy(legacySavePath, backupFile, true);

                if (!File.Exists(backupFile))
                {
                    Debug.LogError("[SaveLoadManager] Tạo bản sao lưu dự phòng thất bại! Hủy thao tác chuyển đổi save cũ.");
                    return;
                }
                Debug.Log($"[SaveLoadManager] Đã tạo bản sao lưu an toàn của savegame.json tại: {backupFile}");

                // Sao chép sang đường dẫn save mới theo tài khoản
                File.Copy(legacySavePath, targetPath, false);

                // Yêu cầu 5: Xác nhận dữ liệu đích đọc được và hợp lệ trước khi tạo marker .migrated
                bool isTargetValid = false;
                try
                {
                    string newContent = File.ReadAllText(targetPath);
                    SaveData parsed = JsonUtility.FromJson<SaveData>(newContent);
                    if (parsed != null && parsed.SaveVersion >= 1 && !string.IsNullOrEmpty(parsed.CurrentMap))
                    {
                        isTargetValid = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveLoadManager] Lỗi kiểm tra dữ liệu save mới sau chuyển đổi: {ex.Message}");
                    isTargetValid = false;
                }

                if (isTargetValid)
                {
                    // Đánh dấu chuyển đổi hoàn tất bằng file marker .migrated khi dữ liệu đích hợp lệ
                    File.WriteAllText(migratedMarker, $"Migrated to account {accountId} at {DateTime.UtcNow:o}");
                    Debug.Log($"[SaveLoadManager] Chuyển đổi thành công save cũ sang tài khoản '{accountId}'. Dữ liệu đích đã được xác nhận toàn vẹn.");
                }
                else
                {
                    // Nếu thất bại: giữ nguyên file gốc, xóa file đích lỗi, KHÔNG tạo marker .migrated
                    Debug.LogError("[SaveLoadManager] Kiểm tra dữ liệu save mới sau chuyển đổi thất bại! Hủy thao tác để đảm bảo an toàn.");
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadManager] Lỗi trong quá trình chuyển đổi save cũ: {ex.Message}");
            }
        }

        public bool HasSaveData()
        {
#if UNITY_EDITOR
            if (DisableDiskPersistenceForTests) return false;
#endif
            // Yêu cầu 2: Khi đã có tài khoản, kiểm tra file save theo tài khoản
            if (!string.IsNullOrEmpty(currentAccountId))
            {
                EnsurePaths();
                return !string.IsNullOrEmpty(saveFilePath) && File.Exists(saveFilePath);
            }

            // Khi chưa có tài khoản đăng nhập (ví dụ kiểm tra ban đầu xem thiết bị có save cũ hay không)
            string legacySavePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            return File.Exists(legacySavePath);
        }

        public void RequestAutoSave()
        {
            if (batchDepth > 0)
            {
                saveRequestedDuringBatch = true;
                return;
            }
            if (isSaving) return;
            SaveGame();
        }

        public void BeginBatch()
        {
            batchDepth++;
        }

        public void EndBatch(bool commit)
        {
            if (batchDepth == 0) return;
            batchDepth--;
            if (batchDepth > 0) return;
            bool shouldSave = commit && saveRequestedDuringBatch;
            saveRequestedDuringBatch = false;
            if (shouldSave) SaveGame();
        }

        public void SaveGame()
        {
#if UNITY_EDITOR
            if (DisableDiskPersistenceForTests) return;
#endif
            // Yêu cầu 2 & 3: Chỉ lưu save theo tài khoản; nếu chưa có tài khoản, không ghi đè savegame.json cũ, giữ file cũ nguyên vẹn
            if (string.IsNullOrEmpty(currentAccountId))
            {
                Debug.LogWarning("[SaveLoadManager] Chưa xác định tài khoản đăng nhập. Từ chối SaveGame() để không ghi đè savegame.json cũ.");
                return;
            }

            EnsurePaths();
            if (string.IsNullOrEmpty(saveFilePath))
            {
                Debug.LogWarning("[SaveLoadManager] Đường dẫn lưu trữ tài khoản không hợp lệ. Từ chối SaveGame().");
                return;
            }

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
                    data.FinalPageConfirmed = GameStateManager.Instance.FinalPageConfirmed;
                    data.GameCompleted = GameStateManager.Instance.GameCompleted;
                    data.CollectedMapFragments = new List<int>(GameStateManager.Instance.CollectedMapFragments);
                    data.PlacedMapFragments = new List<int>(GameStateManager.Instance.PlacedMapFragments);
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
                File.WriteAllText(temporarySaveFilePath, json);
                File.Copy(temporarySaveFilePath, saveFilePath, true);
                File.Delete(temporarySaveFilePath);
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
            // Yêu cầu 2: Chỉ tải save theo tài khoản sau khi luồng đăng nhập gọi SetCurrentAccount thành công
            if (string.IsNullOrEmpty(currentAccountId))
            {
                Debug.LogWarning("[SaveLoadManager] Chưa xác định tài khoản đăng nhập. Từ chối LoadGame().");
                LastLoadStatus = SaveLoadStatus.NoAccount;
                LastErrorMessage = "Chưa xác định tài khoản đăng nhập.";
                return false;
            }

            EnsurePaths();
            if (!HasSaveData())
            {
                Debug.Log("[SaveLoadManager] Không tìm thấy file save để tải.");
                LastLoadStatus = SaveLoadStatus.NoSaveData;
                LastErrorMessage = "Không tìm thấy file save.";
                return false;
            }
            try
            {
                string json = File.ReadAllText(saveFilePath);
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(json.Trim()))
                {
                    Debug.LogError("[SaveLoadManager] Dữ liệu file save rỗng hoặc bị hỏng.");
                    LastLoadStatus = SaveLoadStatus.CorruptedData;
                    LastErrorMessage = "File save rỗng.";
                    return false;
                }
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    Debug.LogError("[SaveLoadManager] Dữ liệu file save bị hỏng.");
                    LastLoadStatus = SaveLoadStatus.CorruptedData;
                    LastErrorMessage = "Dữ liệu JSON không hợp lệ.";
                    return false;
                }
                if (data.SaveVersion < 1 || data.SaveVersion > 2)
                {
                    Debug.LogError($"[SaveLoadManager] SaveVersion không được hỗ trợ: {data.SaveVersion}");
                    LastLoadStatus = SaveLoadStatus.UnsupportedVersion;
                    LastErrorMessage = $"SaveVersion không được hỗ trợ: {data.SaveVersion}";
                    return false;
                }
                if (!Enum.TryParse<MapId>(data.CurrentMap, out var mapId) ||
                    !Enum.TryParse<MapState>(data.CurrentMapState, out var mapState))
                {
                    Debug.LogError("[SaveLoadManager] Map ID hoặc Map State trong save không hợp lệ.");
                    LastLoadStatus = SaveLoadStatus.InvalidMapData;
                    LastErrorMessage = "Map ID hoặc Map State trong save không hợp lệ.";
                    return false;
                }
                BeginBatch();
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
                    GameStateManager.Instance.LoadState(
                            mapId,
                            mapState,
                            data.M1_MiniDone,
                            data.M2_MiniDone,
                            data.M3_MiniDone,
                            data.PedestalPlaced,
                            data.BridgeReady,
                            data.ReunionSeen,
                            data.IntroSeen,
                            data.FinalPageConfirmed,
                            data.GameCompleted,
                            data.CollectedMapFragments,
                            data.PlacedMapFragments
                    );
                }
                bool repaired = RepairMissingQuizRewards();
                if (repaired) RequestAutoSave();
                EndBatch(repaired);
                Debug.Log("[SaveLoadManager] Khôi phục tiến trình thành công từ file save!");
                LastLoadStatus = SaveLoadStatus.Success;
                LastErrorMessage = string.Empty;
                OnGameLoaded?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                batchDepth = 0;
                saveRequestedDuringBatch = false;
                LastLoadStatus = SaveLoadStatus.CorruptedData;
                LastErrorMessage = ex.Message;
                Debug.LogError($"[SaveLoadManager] Lỗi khi nạp dữ liệu save: {ex.Message}");
                // Yêu cầu 6: Tuyệt đối không tự động tạo save trắng hoặc ghi đè file lỗi khi LoadGame() thất bại
                return false;
            }
        }

        private bool RepairMissingQuizRewards()
        {
            if (QuizManager.Instance == null || InventoryManager.Instance == null) return false;
            bool repaired = false;
            foreach (string quizId in QuizManager.Instance.SolvedQuizIds)
            {
                var quiz = QuizManager.Instance.GetQuizData(quizId);
                if (quiz == null || string.IsNullOrEmpty(quiz.RewardItemId)) continue;
                if (IsItemAlreadyCollectedOrPlaced(quiz.RewardItemId)) continue;
                if (InventoryManager.Instance.AddItem(quiz.RewardItemId)) repaired = true;
                else Debug.LogError($"[SaveLoadManager] Không thể phục hồi phần thưởng còn thiếu: {quiz.RewardItemId}");
            }
            return repaired;
        }

        public void DeleteSave()
        {
            // Yêu cầu 7: Đảm bảo DeleteSave() chỉ tác động đến save của tài khoản hiện tại. Nếu chưa có tài khoản, từ chối thao tác.
            if (string.IsNullOrEmpty(currentAccountId))
            {
                Debug.LogWarning("[SaveLoadManager] Chưa xác định tài khoản đăng nhập. Từ chối DeleteSave().");
                return;
            }

            if (HasSaveData())
            {
                File.Delete(saveFilePath);
                Debug.Log($"[SaveLoadManager] Đã xóa file save tiến trình của tài khoản hiện tại: {saveFilePath}");
            }
            if (!string.IsNullOrEmpty(temporarySaveFilePath) && File.Exists(temporarySaveFilePath))
                File.Delete(temporarySaveFilePath);
        }

        public void StartNewGame()
        {
            // Yêu cầu 7: Đảm bảo StartNewGame() chỉ tác động đến save của tài khoản hiện tại. Nếu chưa có tài khoản, từ chối thao tác.
            if (string.IsNullOrEmpty(currentAccountId))
            {
                Debug.LogWarning("[SaveLoadManager] Chưa xác định tài khoản đăng nhập. Từ chối StartNewGame().");
                return;
            }

            DeleteSave();
            InventoryManager.Instance?.Clear();
            QuizManager.Instance?.ResetProgress();
            PlacementManager.Instance?.ResetProgress();
            GameStateManager.Instance?.ResetProgress();
            SaveGame();
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
