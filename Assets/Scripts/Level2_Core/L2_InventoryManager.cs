using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class L2_InventoryManager : MonoBehaviour
{
    public static L2_InventoryManager Instance { get; private set; }

    [Header("Item Sprites (Art Integration Ready)")]
    [SerializeField] public Sprite aoDaiIcon;
    [SerializeField] public Sprite nonLaIcon;
    [SerializeField] public Sprite hanbokIcon;
    [SerializeField] public Sprite muGatIcon;

    [Header("Inventory UI Slots")]
    [SerializeField] private Image[] slotIcons = new Image[5];

    private readonly List<L2_ItemType> items = new List<L2_ItemType>();

    public IReadOnlyList<L2_ItemType> Items => items;

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

    public bool HasItem(L2_ItemType type)
    {
        return items.Contains(type);
    }

    public void AddItem(L2_ItemType type)
    {
        if (!items.Contains(type) && (slotIcons == null || items.Count < slotIcons.Length))
        {
            items.Add(type);
            RefreshUI();
        }
    }

    public void RemoveItem(L2_ItemType type)
    {
        if (items.Contains(type))
        {
            items.Remove(type);
            RefreshUI();
        }
    }

    public Sprite GetItemSprite(L2_ItemType type)
    {
        switch (type)
        {
            case L2_ItemType.AoDai:
                return aoDaiIcon;
            case L2_ItemType.NonLa:
                return nonLaIcon;
            case L2_ItemType.Hanbok:
                return hanbokIcon;
            case L2_ItemType.MuGat:
                return muGatIcon;
            default:
                return null;
        }
    }

    public Color GetFallbackColor(L2_ItemType type)
    {
        switch (type)
        {
            case L2_ItemType.AoDai:
                return new Color(0.9f, 0.15f, 0.15f, 1f); // Red
            case L2_ItemType.NonLa:
                return new Color(0.98f, 0.78f, 0.15f, 1f); // Amber / Gold
            case L2_ItemType.Hanbok:
                return new Color(0.12f, 0.53f, 0.9f, 1f); // Blue
            case L2_ItemType.MuGat:
                return new Color(0.2f, 0.2f, 0.32f, 1f); // Dark Indigo
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

                L2_ItemType type = items[i];
                Sprite icon = GetItemSprite(type);

                if (icon != null)
                {
                    slotIcons[i].sprite = icon;
                    slotIcons[i].color = Color.white;
                    slotIcons[i].preserveAspect = true;
                }
                else
                {
                    // Fallback colored square when sprite asset has not been assigned yet
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

    // Helper method for tests or runtime slot wiring
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
