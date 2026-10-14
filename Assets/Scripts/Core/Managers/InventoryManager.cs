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

        public bool AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[InventoryManager] ItemId không hợp lệ (null/empty).");
                return false;
            }

            if (items.Count >= MaxCapacity)
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
