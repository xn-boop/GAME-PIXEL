using System;
using UnityEngine;

namespace JourneyOfMemories.Core.Managers
{
    public class CombineManager : MonoBehaviour
    {
        public static CombineManager Instance { get; private set; }

        public const string GemLotusHalfId = "Gem_LotusHalf";
        public const string GemMugunghwaHalfId = "Gem_MugunghwaHalf";
        public const string GemWholeId = "Gem_Whole";

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

        /// <summary>
        /// API Hợp đồng: TryCombineGems
        /// Tự động quét túi để ghép hai nửa ngọc thành một viên ngọc hoàn chỉnh.
        /// </summary>
        public (bool success, string resultGemId, string message) TryCombineGems()
        {
            if (InventoryManager.Instance == null)
            {
                return (false, null, "Không tìm thấy InventoryManager.");
            }

            bool hasLotus = InventoryManager.Instance.HasItem(GemLotusHalfId);
            bool hasMugunghwa = InventoryManager.Instance.HasItem(GemMugunghwaHalfId);

            if (!hasLotus || !hasMugunghwa)
            {
                string missing = "";
                if (!hasLotus && !hasMugunghwa) missing = "cả hai nửa ngọc";
                else if (!hasLotus) missing = "nửa ngọc Hoa Sen";
                else missing = "nửa ngọc Mugunghwa";

                return (false, null, $"Chưa thể ghép. Bạn còn thiếu {missing}.");
            }

            // Kiểm tra trạng thái câu đố Q06 và Q07 nếu QuizManager tồn tại
            if (QuizManager.Instance != null)
            {
                bool q06Solved = QuizManager.Instance.IsQuizSolved("Q06");
                bool q07Solved = QuizManager.Instance.IsQuizSolved("Q07");
                if (!q06Solved || !q07Solved)
                {
                    return (false, null, "Cần giải mã ý nghĩa văn hóa của cả hai nửa ngọc trước khi ghép.");
                }
            }

            // Thao tác nguyên tử (Atomic transaction): xóa 2 nửa và thêm 1 ngọc nguyên
            InventoryManager.Instance.RemoveItem(GemLotusHalfId);
            InventoryManager.Instance.RemoveItem(GemMugunghwaHalfId);
            bool added = InventoryManager.Instance.AddItem(GemWholeId);

            if (!added)
            {
                // Rollback nếu có lỗi xảy ra
                InventoryManager.Instance.AddItem(GemLotusHalfId);
                InventoryManager.Instance.AddItem(GemMugunghwaHalfId);
                return (false, null, "Túi đồ không đủ chỗ để nhận ngọc hoàn chỉnh.");
            }

            Debug.Log("[CombineManager] Ghép ngọc thành công: Gem_Whole đã được thêm vào túi đồ.");
            OnGemsCombined?.Invoke();
            SaveLoadManager.Instance?.RequestAutoSave();

            return (true, GemWholeId, "Hai nửa ngọc đã hòa làm một, tạo thành viên ngọc hoàn chỉnh.");
        }
    }
}
