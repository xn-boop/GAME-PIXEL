using System;
using UnityEngine;
using System.Collections.Generic;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    public class CombineManager : MonoBehaviour
    {
        public static CombineManager Instance { get; private set; }

        public const string GemLotusHalfId = GameIds.Item.LotusHalf;
        public const string GemMugunghwaHalfId = GameIds.Item.MugunghwaHalf;
        public const string GemWholeId = GameIds.Item.WholeGem;

        public event Action OnGemsCombined;

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

        /// <summary>
        /// API Hợp đồng: TryCombineGems
        /// Tự động quét túi để ghép hai nửa ngọc thành một viên ngọc hoàn chỉnh.
        /// </summary>
        public (CombineResult result, string resultGemId, string message) TryCombineGems()
        {
            if (InventoryManager.Instance == null)
            {
                return (CombineResult.InventoryUnavailable, null, "Không tìm thấy InventoryManager.");
            }

            if (InventoryManager.Instance.HasItem(GemWholeId))
                return (CombineResult.AlreadyCombined, GemWholeId, "Viên ngọc đã được ghép trước đó.");

            bool hasLotus = InventoryManager.Instance.HasItem(GemLotusHalfId);
            bool hasMugunghwa = InventoryManager.Instance.HasItem(GemMugunghwaHalfId);

            if (!hasLotus || !hasMugunghwa)
            {
                string missing = "";
                if (!hasLotus && !hasMugunghwa) missing = "cả hai nửa ngọc";
                else if (!hasLotus) missing = "nửa ngọc Hoa Sen";
                else missing = "nửa ngọc Mugunghwa";

                return (CombineResult.MissingParts, null, $"Chưa thể ghép. Bạn còn thiếu {missing}.");
            }

            // Kiểm tra trạng thái câu đố Q06 và Q07 nếu QuizManager tồn tại
            if (QuizManager.Instance != null)
            {
                bool q06Solved = QuizManager.Instance.IsQuizSolved("Q06");
                bool q07Solved = QuizManager.Instance.IsQuizSolved("Q07");
                if (!q06Solved || !q07Solved)
                {
                    return (CombineResult.QuizzesNotSolved, null, "Cần giải mã ý nghĩa văn hóa của cả hai nửa ngọc trước khi ghép.");
                }
            }

            SaveLoadManager.Instance?.BeginBatch();
            bool replaced = InventoryManager.Instance.TryReplaceItems(
                new List<string> { GemLotusHalfId, GemMugunghwaHalfId },
                GemWholeId);
            if (!replaced)
            {
                SaveLoadManager.Instance?.EndBatch(false);
                return (CombineResult.InventoryUnavailable, null, "Không thể hoàn tất giao dịch ghép ngọc.");
            }

            Debug.Log("[CombineManager] Ghép ngọc thành công: Gem_Whole đã được thêm vào túi đồ.");
            OnGemsCombined?.Invoke();
            SaveLoadManager.Instance?.RequestAutoSave();
            SaveLoadManager.Instance?.EndBatch(true);

            return (CombineResult.Success, GemWholeId, "Hai nửa ngọc đã hòa làm một, tạo thành viên ngọc hoàn chỉnh.");
        }
    }
}
