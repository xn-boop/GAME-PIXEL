using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Map3
{
    public class Map3_InventoryManager : MonoBehaviour
    {
        public static Map3_InventoryManager Instance { get; private set; }

        public const int MaxSlots = 5;

        [Header("Item Sprites (Optional Icons)")]
        [SerializeField] public Sprite keyIcon;
        [SerializeField] public Sprite woodPlankIcon;
        [SerializeField] public Sprite vietJadeIcon;
        [SerializeField] public Sprite prayerStoneIcon;
        [SerializeField] public Sprite koreaJadeIcon;

        [Header("UI Slot References (5 Slots)")]
        [SerializeField] private Image[] slotIcons = new Image[MaxSlots];
        [SerializeField] private Image[] slotBackgrounds = new Image[MaxSlots];
        [SerializeField] private TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[MaxSlots];

        private readonly List<string> items = new List<string>();

        public IReadOnlyList<string> Items => items;
        public int Count => items.Count;
        public bool IsFull => items.Count >= MaxSlots;

        public event Action OnInventoryChanged;

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
            RefreshUI();
        }

        public bool HasItem(string itemId)
        {
            return items.Contains(itemId);
        }

        public int GetItemCount(string itemId)
        {
            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == itemId) count++;
            }
            return count;
        }

        public bool AddItem(string itemId)
        {
            if (items.Count >= MaxSlots)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Túi đồ đã đầy (5/5 ô)!");
                }
                return false;
            }

            items.Add(itemId);
            RefreshUI();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(string itemId, int count = 1)
        {
            int removed = 0;
            for (int i = items.Count - 1; i >= 0 && removed < count; i--)
            {
                if (items[i] == itemId)
                {
                    items.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
            {
                RefreshUI();
                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void ClearAll()
        {
            items.Clear();
            RefreshUI();
            OnInventoryChanged?.Invoke();
        }

        public Sprite GetItemSprite(string itemId)
        {
            switch (itemId)
            {
                case Map3_ItemIds.Key:
                    return keyIcon;
                case Map3_ItemIds.WoodPlank:
                    return woodPlankIcon;
                case Map3_ItemIds.VietJade:
                    return vietJadeIcon;
                case Map3_ItemIds.PrayerStone:
                    return prayerStoneIcon;
                case Map3_ItemIds.KoreaJade:
                    return koreaJadeIcon;
                default:
                    return null;
            }
        }

        public Color GetItemColor(string itemId)
        {
            switch (itemId)
            {
                case Map3_ItemIds.Key:
                    return new Color(1f, 0.84f, 0f, 1f); // Gold
                case Map3_ItemIds.WoodPlank:
                    return new Color(0.65f, 0.45f, 0.28f, 1f); // Wood Brown
                case Map3_ItemIds.VietJade:
                    return new Color(0.0f, 0.9f, 0.46f, 1f); // Emerald Jade Green
                case Map3_ItemIds.PrayerStone:
                    return new Color(0.6f, 0.7f, 0.8f, 1f); // Slate Pebble
                case Map3_ItemIds.KoreaJade:
                    return new Color(1.0f, 0.35f, 0.65f, 1f); // Mugunghwa Pink
                default:
                    return Color.white;
            }
        }

        public string GetItemDisplayName(string itemId)
        {
            switch (itemId)
            {
                case Map3_ItemIds.Key:
                    return "Chìa khóa";
                case Map3_ItemIds.WoodPlank:
                    return "Mảnh gỗ";
                case Map3_ItemIds.VietJade:
                    return "Ngọc Việt";
                case Map3_ItemIds.PrayerStone:
                    return "Đá nguyện";
                case Map3_ItemIds.KoreaJade:
                    return "Ngọc Hàn";
                default:
                    return itemId;
            }
        }

        public void RefreshUI()
        {
            if (slotIcons == null) return;

            for (int i = 0; i < slotIcons.Length; i++)
            {
                if (slotIcons[i] == null) continue;

                if (i < items.Count)
                {
                    string id = items[i];
                    Sprite spr = GetItemSprite(id);

                    slotIcons[i].gameObject.SetActive(true);
                    if (spr != null)
                    {
                        slotIcons[i].sprite = spr;
                        slotIcons[i].color = Color.white;
                    }
                    else
                    {
                        slotIcons[i].color = GetItemColor(id);
                    }

                    if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                    {
                        slotLabels[i].text = GetItemDisplayName(id);
                        slotLabels[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    slotIcons[i].gameObject.SetActive(false);
                    if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                    {
                        slotLabels[i].text = "";
                        slotLabels[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
