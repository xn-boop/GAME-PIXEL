using System;
using System.Collections.Generic;
using UnityEngine;

namespace JourneyOfMemories.Core.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        public const int MaxCapacity = 5;

        [SerializeField]
        private List<string> items = new List<string>();

        public event Action<IReadOnlyList<string>> OnInventoryUpdated;

        public IReadOnlyList<string> Items => items.AsReadOnly();
        public int ItemCount => items.Count;
        public int Capacity => MaxCapacity;

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

        public bool AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[InventoryManager] ItemId không hợp lệ (null/empty).");
                return false;
            }

            if (items.Contains(itemId))
            {
                Debug.LogWarning($"[InventoryManager] Vật phẩm nhiệm vụ đã tồn tại: {itemId}");
                return false;
            }

            if (!CanAddItem(itemId))
            {
                Debug.LogWarning($"[InventoryManager] Túi đồ đã đầy ({MaxCapacity}/{MaxCapacity}). Không thể thêm: {itemId}");
                return false;
            }

            items.Add(itemId);
            Debug.Log($"[InventoryManager] Đã thêm vật phẩm: {itemId}. Số lượng hiện tại: {items.Count}/{MaxCapacity}");

            OnInventoryUpdated?.Invoke(items.AsReadOnly());
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool CanAddItem(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) &&
                   !items.Contains(itemId) &&
                   items.Count < MaxCapacity;
        }

        /// <summary>
        /// Thay nhiều vật phẩm trong một mutation duy nhất. Dùng cho ghép ngọc để save
        /// không bao giờ thấy trạng thái mới xóa một nửa.
        /// </summary>
        public bool TryReplaceItems(IReadOnlyList<string> removeIds, string addItemId)
        {
            if (removeIds == null || removeIds.Count == 0 || string.IsNullOrEmpty(addItemId))
                return false;

            if (items.Contains(addItemId))
                return false;

            var candidate = new List<string>(items);
            foreach (var removeId in removeIds)
            {
                if (string.IsNullOrEmpty(removeId) || !candidate.Remove(removeId))
                    return false;
            }

            if (candidate.Count >= MaxCapacity)
                return false;

            candidate.Add(addItemId);
            items = candidate;
            OnInventoryUpdated?.Invoke(items.AsReadOnly());
            SaveLoadManager.Instance?.RequestAutoSave();
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            bool removed = items.Remove(itemId);
            if (removed)
            {
                Debug.Log($"[InventoryManager] Đã xóa vật phẩm: {itemId}. Số lượng hiện tại: {items.Count}/{MaxCapacity}");
                OnInventoryUpdated?.Invoke(items.AsReadOnly());
                SaveLoadManager.Instance?.RequestAutoSave();
            }
            return removed;
        }

        public bool HasItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return items.Contains(itemId);
        }

        public void Clear()
        {
            items.Clear();
            OnInventoryUpdated?.Invoke(items.AsReadOnly());
        }

        public void LoadItems(IEnumerable<string> loadedItems)
        {
            items.Clear();
            if (loadedItems != null)
            {
                foreach (var item in loadedItems)
                {
                    if (items.Count < MaxCapacity && !string.IsNullOrEmpty(item))
                    {
                        items.Add(item);
                    }
                }
            }
            OnInventoryUpdated?.Invoke(items.AsReadOnly());
        }
    }
}
