using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class L1_InventoryManager : MonoBehaviour
{
    public static L1_InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<L1_InventoryManager>();
            }
            return instance;
        }
        private set => instance = value;
    }
    private static L1_InventoryManager instance;

    [Header("Item Sprites")]
    [SerializeField] public Sprite thitKhoIcon;
    [SerializeField] public Sprite kimchiIcon;
    [SerializeField] public Sprite kimbapIcon;

    [Header("Inventory UI Slots")]
    [SerializeField] private Image[] slotIcons = new Image[5];

    private readonly List<L1_ItemType> items = new List<L1_ItemType>();

    public IReadOnlyList<L1_ItemType> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RefreshUI();
    }

    public bool HasItem(L1_ItemType type)
    {
        return items.Contains(type);
    }

    public void AddItem(L1_ItemType type)
    {
        if (!items.Contains(type) && (slotIcons == null || items.Count < slotIcons.Length))
        {
            items.Add(type);
            RefreshUI();
        }
    }

    public void RemoveItem(L1_ItemType type)
    {
        if (items.Contains(type))
        {
            items.Remove(type);
            RefreshUI();
        }
    }

    public Sprite GetItemSprite(L1_ItemType type)
    {
        switch (type)
        {
            case L1_ItemType.ThitKho:
                return thitKhoIcon;
            case L1_ItemType.Kimchi:
                return kimchiIcon;
            case L1_ItemType.Kimbap:
                return kimbapIcon;
            default:
                return null;
        }
    }

    public Color GetFallbackColor(L1_ItemType type)
    {
        switch (type)
        {
            case L1_ItemType.ThitKho:
                return new Color(0.85f, 0.45f, 0.15f, 1f); // Warm meat amber
            case L1_ItemType.Kimchi:
                return new Color(0.92f, 0.22f, 0.15f, 1f); // Chili red
            case L1_ItemType.Kimbap:
                return new Color(0.2f, 0.5f, 0.25f, 1f);   // Seaweed dark green
            default:
                return Color.white;
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
                slotIcons[i].gameObject.SetActive(true);
                slotIcons[i].enabled = true;

                L1_ItemType type = items[i];
                Sprite icon = GetItemSprite(type);

                if (icon != null)
                {
                    slotIcons[i].sprite = icon;
                    slotIcons[i].color = Color.white;
                    slotIcons[i].preserveAspect = true;
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].color = GetFallbackColor(type);
                }
            }
            else
            {
                slotIcons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetSlotIcons(Image[] icons)
    {
        slotIcons = icons;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
