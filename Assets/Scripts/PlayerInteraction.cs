
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField, Min(0.1f)]
    private float interactionRadius = 1.8f;

    [SerializeField]
    private KeyCode interactKey = KeyCode.E;

    [Header("Inventory")]
    [SerializeField]
    private InventoryUI inventoryUI;

    [Header("Interaction Lock")]
    [SerializeField]
    private bool isInteractionLocked = false;

    private FoodItem nearbyFood;
    private FoodSlot nearbySlot;

    private string lastMessage = "";

    private void Update()
    {
        // Không xử lý nhặt hoặc đặt món khi đang bị khóa.
        if (isInteractionLocked)
            return;

        FindNearbyObjects();

        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void FindNearbyObjects()
    {
        nearbyFood = null;
        nearbySlot = null;

        // Tìm món ăn chưa được nhặt gần nhất.
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRadius
        );

        float closestFoodDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            FoodItem food = hit.GetComponent<FoodItem>();

            if (food == null || food.IsCollected)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                food.transform.position
            );

            if (distance < closestFoodDistance)
            {
                closestFoodDistance = distance;
                nearbyFood = food;
            }
        }

        // Tìm ô đặt món gần nhất còn trống.
        FoodSlot[] allSlots =
            FindObjectsByType<FoodSlot>(FindObjectsSortMode.None);

        float closestSlotDistance = float.MaxValue;

        foreach (FoodSlot slot in allSlots)
        {
            if (slot == null || slot.IsCompleted)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                slot.transform.position
            );

            if (distance <= interactionRadius &&
                distance < closestSlotDistance)
            {
                closestSlotDistance = distance;
                nearbySlot = slot;
            }
        }
    }

    public void Interact()
    {
        // Chặn cả trường hợp Interact được gọi từ UI.
        if (isInteractionLocked)
            return;

        // Kiểm tra InventoryUI.
        if (inventoryUI == null)
        {
            ShowMessage(
                "Lỗi cấu hình: chưa gán InventoryUI vào PlayerInteraction!",
                true
            );
            return;
        }

        FoodItem selectedFood = inventoryUI.SelectedItem;

        // Đang chọn món thì ưu tiên đặt món đó.
        if (selectedFood != null)
        {
            TryPlaceFood(selectedFood);
            return;
        }

        // Chưa chọn món và đang ở gần mâm.
        if (nearbySlot != null)
        {
            if (inventoryUI.ItemCount == 0)
            {
                ShowMessage(
                    "Inventory đang trống. Hãy tìm và nhặt món ăn trước!"
                );
            }
            else
            {
                ShowMessage(
                    "Vui lòng chọn món ăn trong Inventory để đặt vào mâm!"
                );
            }

            return;
        }

        // Chưa chọn món và ở gần đồ ăn.
        if (nearbyFood != null)
        {
            PickUpFood(nearbyFood);
            return;
        }

        // Không ở gần đồ ăn hoặc ô đặt món.
        if (inventoryUI.ItemCount == 0)
        {
            ShowMessage(
                "Không có món ăn nào ở gần. Hãy di chuyển để tìm đồ ăn!"
            );
        }
        else
        {
            ShowMessage(
                "Hãy đến gần món ăn để nhặt, hoặc chọn món trong Inventory để đặt!"
            );
        }
    }

    private void PickUpFood(FoodItem food)
    {
        if (isInteractionLocked)
            return;

        if (food == null)
        {
            ShowMessage("Không tìm thấy món ăn để nhặt!");
            return;
        }

        if (food.IsCollected)
        {
            ShowMessage("Món ăn này đã được nhặt rồi!");
            return;
        }

        // Thêm vào Inventory trước khi xác nhận nhặt.
        bool added = inventoryUI.AddItem(food);

        if (!added)
        {
            ShowMessage(
                "Inventory đã đầy! Hãy đặt bớt món trước khi nhặt thêm."
            );
            return;
        }

        food.Collect(transform);

        ShowMessage("Đã nhặt: " + food.FoodName);
    }

    private void TryPlaceFood(FoodItem food)
    {
        if (isInteractionLocked)
            return;

        if (food == null)
        {
            ShowMessage(
                "Món đang chọn không hợp lệ. Hãy chọn lại trong Inventory!"
            );
            return;
        }

        // Chưa đến gần ô đặt món.
        if (nearbySlot == null)
        {
            ShowMessage(
                "Bạn đang chọn " + food.FoodName +
                ". Hãy đến gần một ô trống trên mâm cơm!"
            );
            return;
        }

        // Thử đặt món vào ô trống gần nhất.
        bool success = nearbySlot.TryPlaceFood(food);

        if (success)
        {
            inventoryUI.RemoveItem(food);

            ShowMessage("Đặt món thành công: " + food.FoodName);
        }
        else
        {
            // Đặt sai thì giữ nguyên món trong Inventory.
            ShowMessage(
                "Sai món! " + food.FoodName +
                " vẫn ở trong Inventory. Hãy thử ô khác!"
            );
        }
    }

    // Gọi hàm này từ MemorySceneController để khóa/mở tương tác.
    public void SetInteractionLocked(bool locked)
    {
        isInteractionLocked = locked;

        if (locked)
        {
            nearbyFood = null;
            nearbySlot = null;
            Debug.Log("PlayerInteraction: Đã khóa tương tác.");
        }
        else
        {
            lastMessage = "";
            Debug.Log("PlayerInteraction: Đã mở khóa tương tác.");
        }
    }

    private void ShowMessage(
        string message,
        bool isError = false
    )
    {
        // Tránh ghi cùng một thông báo liên tục.
        if (message == lastMessage)
            return;

        lastMessage = message;

        if (isError)
        {
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRadius
        );
    }
}
