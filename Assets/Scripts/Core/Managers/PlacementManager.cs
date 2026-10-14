using System;
using System.Collections.Generic;
using UnityEngine;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        // Bảng quy định Slot -> Vật phẩm hợp lệ
        private readonly Dictionary<string, string> slotConfig = new Dictionary<string, string>
        {
            // Map 1: Mâm cơm 3 món
            { "Slot_ThitKhoTrung", "Food_ThitKhoTrung" },
            { "Slot_Kimchi", "Food_Kimchi" },
            { "Slot_ComCuon", "Food_ComCuon" },

            // Map 2: Ma nơ canh ba mẹ
            { "Slot_Mother", "Clothing_AoDai" },
            { "Slot_Father", "Clothing_Hanbok" },

            // Map 3: Bệ đá
            { "Slot_Pedestal", "Gem_Whole" }
        };

        // Danh sách các slot đã đặt thành công (slotId -> itemId)
        [SerializeField]
        private Dictionary<string, string> filledSlots = new Dictionary<string, string>();

        public event Action<string, string> OnItemPlaced; // slotId, itemId

        public IReadOnlyDictionary<string, string> FilledSlots => filledSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterSlotConfig(string slotId, string requiredItemId)
        {
            if (string.IsNullOrEmpty(slotId) || string.IsNullOrEmpty(requiredItemId)) return;
            slotConfig[slotId] = requiredItemId;
        }

        public bool IsSlotFilled(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return false;
            return filledSlots.ContainsKey(slotId);
        }

        public string GetItemInSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return null;
            filledSlots.TryGetValue(slotId, out var itemId);
            return itemId;
        }

        /// <summary>
        /// API Hợp đồng: TryPlaceItem
        /// </summary>
        public PlacementResult TryPlaceItem(string itemId, string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                return PlacementResult.InvalidSlot;
            }

            // 1. Kiểm tra Slot đích đã có đồ chưa
            if (IsSlotFilled(slotId))
            {
                Debug.LogWarning($"[PlacementManager] Vị trí {slotId} đã được lấp đầy trước đó.");
                return PlacementResult.SlotAlreadyFilled;
            }

            // 2. Kiểm tra vật phẩm có trong túi đồ không
            if (InventoryManager.Instance != null && !InventoryManager.Instance.HasItem(itemId))
            {
                Debug.LogWarning($"[PlacementManager] Người chơi không sở hữu vật phẩm '{itemId}' trong túi đồ.");
                return PlacementResult.ItemNotInInventory;
            }

            // 3. So khớp ID Vật phẩm với cấu hình cho phép
            if (!slotConfig.TryGetValue(slotId, out var requiredItemId))
            {
                Debug.LogWarning($"[PlacementManager] Slot '{slotId}' chưa được cấu hình.");
                return PlacementResult.InvalidSlot;
            }

            // Nhánh Sai
            if (itemId != requiredItemId)
            {
                Debug.Log($"[PlacementManager] Vật phẩm '{itemId}' không khớp với vị trí '{slotId}' (cần '{requiredItemId}').");
                return PlacementResult.ItemMismatch;
            }

            // Nhánh Đúng
            filledSlots[slotId] = itemId;
            Debug.Log($"[PlacementManager] Đặt thành công '{itemId}' vào vị trí '{slotId}'.");

            // Tiêu thụ vật phẩm khỏi túi đồ
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(itemId);
            }

            OnItemPlaced?.Invoke(slotId, itemId);
            GameStateManager.Instance?.CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();

            return PlacementResult.Success;
        }

        public void LoadFilledSlots(IDictionary<string, string> loadedSlots)
        {
            filledSlots.Clear();
            if (loadedSlots != null)
            {
                foreach (var kvp in loadedSlots)
                {
                    filledSlots[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
