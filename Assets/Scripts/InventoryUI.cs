using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JourneyOfMemories.Core.Managers;

/// <summary>UI adapter; InventoryManager is the only gameplay source of truth.</summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Slots")]
    [SerializeField] private Image[] itemIcons = new Image[5];
    [SerializeField] private Button[] slotButtons = new Button[5];
    [Header("Selection Visual")]
    [SerializeField] private Image[] slotBackgrounds = new Image[5];
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.4f, 1f);

    private readonly Dictionary<string, Sprite> iconByItemId = new Dictionary<string, Sprite>();
    private int selectedIndex = -1;

    public string SelectedItemId
    {
        get
        {
            var inventory = InventoryManager.Instance;
            return inventory != null && selectedIndex >= 0 && selectedIndex < inventory.Items.Count
                ? inventory.Items[selectedIndex]
                : null;
        }
    }

    public int ItemCount => InventoryManager.Instance != null ? InventoryManager.Instance.ItemCount : 0;

    private void Awake()
    {
        FoodItem[] sources = FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var source in sources)
            if (source != null && source.InventoryIcon != null) iconByItemId[source.ItemId] = source.InventoryIcon;
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryUpdated += OnInventoryUpdated;
    }

    private void Start()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int captured = i;
            if (slotButtons[i] != null) slotButtons[i].onClick.AddListener(() => SelectSlot(captured));
        }
        RefreshAllSlots();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryUpdated -= OnInventoryUpdated;
    }

    private void OnInventoryUpdated(IReadOnlyList<string> items)
    {
        if (selectedIndex >= items.Count) selectedIndex = -1;
        RefreshAllSlots();
    }

    public void SelectSlot(int index)
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || index < 0 || index >= inventory.Items.Count) return;
        selectedIndex = selectedIndex == index ? -1 : index;
        RefreshAllSlots();
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
        RefreshAllSlots();
    }

    public bool AddItem(FoodItem item)
    {
        if (item == null || InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.HasItem(item.ItemId) || InventoryManager.Instance.AddItem(item.ItemId);
    }

    public void RemoveItem(FoodItem item)
    {
        if (item != null) InventoryManager.Instance?.RemoveItem(item.ItemId);
    }

    private void RefreshAllSlots()
    {
        var items = InventoryManager.Instance != null ? InventoryManager.Instance.Items : null;
        for (int i = 0; i < itemIcons.Length; i++)
        {
            string itemId = items != null && i < items.Count ? items[i] : null;
            if (itemIcons[i] != null)
            {
                itemIcons[i].sprite = itemId != null && iconByItemId.TryGetValue(itemId, out var icon) ? icon : null;
                itemIcons[i].enabled = itemIcons[i].sprite != null;
                itemIcons[i].preserveAspect = true;
            }
            if (i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                slotBackgrounds[i].color = selectedIndex == i ? selectedColor : normalColor;
        }
    }
}
