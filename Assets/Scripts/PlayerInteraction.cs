
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

        string selectedItemId = inventoryUI.SelectedItemId;

        // Đang chọn món thì ưu tiên đặt món đó.
        if (!string.IsNullOrEmpty(selectedItemId))
        {
            TryPlaceItem(selectedItemId);
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

        // Kiểm tra điều kiện hoàn thành Mini-game mèo trước khi thu thập
        if (JourneyOfMemories.Core.Managers.GameStateManager.Instance != null &&
            !JourneyOfMemories.Core.Managers.GameStateManager.Instance.M1_MiniDone)
        {
            ShowMessage("Hãy vuốt ve chú mèo để tìm manh mối trước khi lấy món ăn!");
            return;
        }

        // Kiểm tra câu đố văn hóa nếu có
        string quizId = food.QuizId;
        if (string.IsNullOrEmpty(quizId))
        {
            if (food.ItemId == JourneyOfMemories.Core.Data.GameIds.Item.ThitKhoTrung) quizId = JourneyOfMemories.Core.Data.GameIds.Quiz.Q01;
            else if (food.ItemId == JourneyOfMemories.Core.Data.GameIds.Item.Kimchi) quizId = JourneyOfMemories.Core.Data.GameIds.Quiz.Q02;
            else if (food.ItemId == JourneyOfMemories.Core.Data.GameIds.Item.ComCuon) quizId = JourneyOfMemories.Core.Data.GameIds.Quiz.Q03;
        }

        if (!string.IsNullOrEmpty(quizId) && JourneyOfMemories.Core.Managers.QuizManager.Instance != null)
        {
            if (!JourneyOfMemories.Core.Managers.QuizManager.Instance.IsQuizSolved(quizId))
            {
                if (JourneyOfMemories.Core.UI.QuizUIController.Instance != null)
                {
                    JourneyOfMemories.Core.UI.QuizUIController.Instance.OpenQuiz(quizId);
                    return;
                }
            }
        }

        // QuizManager đã cấp vật phẩm cho Core. Với vật phẩm không có quiz,
        // PlayerInteraction mới gửi yêu cầu thêm vào Core.
        var coreInventory = JourneyOfMemories.Core.Managers.InventoryManager.Instance;
        if (coreInventory == null) return;
        if (!coreInventory.HasItem(food.ItemId) && !coreInventory.AddItem(food.ItemId))
        {
            ShowMessage("Inventory đã đầy! Hãy đặt bớt món trước khi nhặt thêm.");
            return;
        }

        food.MarkCollectedAndHide();

        ShowMessage("Đã nhặt: " + food.FoodName);
    }

    private void TryPlaceItem(string itemId)
    {
        if (isInteractionLocked)
            return;

        if (string.IsNullOrEmpty(itemId))
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
                "Bạn đang chọn " + itemId +
                ". Hãy đến gần một ô trống trên mâm cơm!"
            );
            return;
        }

        // Thử đặt món vào ô trống gần nhất.
        bool success = nearbySlot.TryPlaceItem(itemId);

        if (success)
        {
            inventoryUI.ClearSelection();
            ShowMessage("Đặt vật phẩm thành công: " + itemId);
        }
        else
        {
            // Đặt sai thì giữ nguyên món trong Inventory.
            ShowMessage(
                "Sai vật phẩm! " + itemId +
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
