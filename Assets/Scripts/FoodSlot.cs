
using UnityEngine;

public class FoodSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private string requiredFoodName;
    [SerializeField] private string slotId;

    private bool isCompleted = false;

    public bool IsCompleted => isCompleted;
    public string SlotId => slotId;

    private void Start()
    {
        // Tự động suy ra slotId nếu chưa gán
        if (string.IsNullOrEmpty(slotId))
        {
            if (requiredFoodName.Contains("Thịt kho") || requiredFoodName.Contains("ThitKho")) slotId = "Slot_ThitKhoTrung";
            else if (requiredFoodName.Contains("Kim chi") || requiredFoodName.Contains("Kimchi")) slotId = "Slot_Kimchi";
            else if (requiredFoodName.Contains("Kimbap") || requiredFoodName.Contains("ComCuon")) slotId = "Slot_ComCuon";
            else slotId = "Slot_" + requiredFoodName;
        }

        // Đăng ký cấu hình với PlacementManager
        if (JourneyOfMemories.Core.Managers.PlacementManager.Instance != null)
        {
            JourneyOfMemories.Core.Managers.PlacementManager.Instance.RegisterSlotConfig(slotId, requiredFoodName);

            // Kiểm tra trạng thái đã hoàn thành khi nạp game
            if (JourneyOfMemories.Core.Managers.PlacementManager.Instance.IsSlotFilled(slotId))
            {
                isCompleted = true;
            }
        }
    }

    public bool TryPlaceFood(FoodItem food)
    {
        // Kiểm tra món ăn hợp lệ
        if (food == null || isCompleted)
        {
            return false;
        }

        // Ưu tiên ủy quyền phân xử cho PlacementManager nếu có
        if (JourneyOfMemories.Core.Managers.PlacementManager.Instance != null)
        {
            var result = JourneyOfMemories.Core.Managers.PlacementManager.Instance.TryPlaceItem(food.FoodName, slotId);
            if (result != JourneyOfMemories.Core.Data.PlacementResult.Success)
            {
                Debug.Log($"Sai món! Vị trí '{slotId}' cần: {requiredFoodName}. Kết quả: {result}");
                return false;
            }
        }
        else
        {
            // Kiểm tra món ăn có đúng ô không (fallback)
            if (food.FoodName != requiredFoodName)
            {
                Debug.Log("Sai món! Ô này cần: " + requiredFoodName);
                return false;
            }
        }

        // Đánh dấu ô đã hoàn thành
        isCompleted = true;

        // Đặt món ăn vào đúng vị trí
        food.transform.SetParent(transform);
        food.transform.position = transform.position;
        food.transform.localScale = Vector3.one * 0.5f;

        // Cập nhật tiến độ màn chơi
        LevelManager levelManager =
            FindAnyObjectByType<LevelManager>();

        if (levelManager != null)
        {
            levelManager.RegisterCompletedSlot();
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy LevelManager trong Scene!"
            );
        }

        Debug.Log("Đặt đúng món: " + requiredFoodName);

        return true;
    }
}
