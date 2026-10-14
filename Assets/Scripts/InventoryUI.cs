using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Slots")]
    [SerializeField]
    private Image[] itemIcons = new Image[5];

    [SerializeField]
    private Button[] slotButtons = new Button[5];

    [Header("Selection Visual")]
    [SerializeField]
    private Image[] slotBackgrounds = new Image[5];

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color selectedColor =
        new Color(1f, 0.85f, 0.4f, 1f);

    private FoodItem[] inventoryItems = new FoodItem[5];

    private int selectedIndex = -1;

    // Món hiện đang được chọn.
    public FoodItem SelectedItem
    {
        get
        {
            if (selectedIndex < 0 ||
                selectedIndex >= inventoryItems.Length)
            {
                return null;
            }

            return inventoryItems[selectedIndex];
        }
    }

    public string SelectedItemId => SelectedItem != null ? SelectedItem.FoodName : null;

    // Số lượng món đang có trong Inventory.
    public int ItemCount
    {
        get
        {
            if (JourneyOfMemories.Core.Managers.InventoryManager.Instance != null)
            {
                return JourneyOfMemories.Core.Managers.InventoryManager.Instance.ItemCount;
            }

            int count = 0;

            foreach (FoodItem item in inventoryItems)
            {
                if (item != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void OnEnable()
    {
        if (JourneyOfMemories.Core.Managers.InventoryManager.Instance != null)
        {
            JourneyOfMemories.Core.Managers.InventoryManager.Instance.OnInventoryUpdated += OnCoreInventoryUpdated;
        }
    }

    private void OnDisable()
    {
        if (JourneyOfMemories.Core.Managers.InventoryManager.Instance != null)
        {
            JourneyOfMemories.Core.Managers.InventoryManager.Instance.OnInventoryUpdated -= OnCoreInventoryUpdated;
        }
    }

    private void OnCoreInventoryUpdated(System.Collections.Generic.IReadOnlyList<string> items)
    {
        RefreshAllSlots();
    }

    private void Start()
    {
        // Đăng ký sự kiện nhấn từng ô Inventory.
        if (slotButtons != null)
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int index = i;

                if (slotButtons[i] != null)
                {
                    slotButtons[i].onClick.AddListener(
                        () => SelectSlot(index)
                    );
                }
            }
        }

        RefreshAllSlots();
    }

    public bool AddItem(FoodItem item)
    {
        if (item == null)
        {
            Debug.LogWarning(
                "Không thể thêm món null vào Inventory!"
            );
            return false;
        }

        // Tránh thêm cùng một món hai lần.
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == item)
            {
                // Nếu món đã tồn tại, chọn lại món đó.
                selectedIndex = i;
                RefreshAllSlots();
                return true;
            }
        }

        // Tìm ô trống.
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = item;

                // Tự động chọn món vừa nhặt.
                selectedIndex = i;

                RefreshAllSlots();

                Debug.Log(
                    "Đã thêm vào Inventory: " + item.FoodName
                );

                if (JourneyOfMemories.Core.Managers.InventoryManager.Instance != null &&
                    !JourneyOfMemories.Core.Managers.InventoryManager.Instance.HasItem(item.FoodName))
                {
                    JourneyOfMemories.Core.Managers.InventoryManager.Instance.AddItem(item.FoodName);
                }

                return true;
            }
        }

        Debug.Log("Inventory đã đầy!");
        return false;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= inventoryItems.Length)
            return;

        // Không thể chọn ô trống.
        if (inventoryItems[index] == null)
            return;

        // Chạm lại món đang chọn để bỏ chọn.
        if (selectedIndex == index)
        {
            selectedIndex = -1;

            Debug.Log("Đã bỏ chọn món ăn.");
        }
        else
        {
            // Chọn món khác hoặc chuyển lựa chọn.
            selectedIndex = index;

            Debug.Log(
                "Đang chọn: " + inventoryItems[index].FoodName
            );
        }

        RefreshAllSlots();
    }

    public void RemoveItem(FoodItem item)
    {
        if (item == null)
            return;

        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] != item)
                continue;

            inventoryItems[i] = null;

            // Nếu xóa món đang chọn, bỏ lựa chọn.
            if (selectedIndex == i)
            {
                selectedIndex = -1;
            }

            RefreshAllSlots();

            Debug.Log(
                "Đã xóa món khỏi Inventory: " + item.FoodName
            );

            if (JourneyOfMemories.Core.Managers.InventoryManager.Instance != null)
            {
                JourneyOfMemories.Core.Managers.InventoryManager.Instance.RemoveItem(item.FoodName);
            }

            return;
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            UpdateSlotUI(i);
        }
    }

    private void UpdateSlotUI(int index)
    {
        // Cập nhật biểu tượng món ăn.
        if (itemIcons != null &&
            index < itemIcons.Length &&
            itemIcons[index] != null)
        {
            FoodItem item = inventoryItems[index];

            Image icon = itemIcons[index];

            icon.sprite = item != null
                ? item.InventoryIcon
                : null;

            icon.enabled =
                item != null && item.InventoryIcon != null;

            icon.preserveAspect = true;
        }

        // Cập nhật màu nền của ô đang chọn.
        if (slotBackgrounds != null &&
            index < slotBackgrounds.Length &&
            slotBackgrounds[index] != null)
        {
            slotBackgrounds[index].color =
                selectedIndex == index
                    ? selectedColor
                    : normalColor;
        }
    }

    private void OnDestroy()
    {
        // Gỡ listener do script đăng ký để tránh đăng ký trùng
        // khi component được khởi tạo lại trong cùng phiên.
        if (slotButtons == null)
            return;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;

            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.RemoveListener(
                    () => SelectSlot(index)
                );
            }
        }
    }
}