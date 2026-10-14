
using UnityEngine;
using JourneyOfMemories.Core.Data;

public class FoodSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private string requiredFoodName;
    [SerializeField] private string slotId;
    [SerializeField] private string requiredItemId;

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
            string coreRequiredId = JourneyOfMemories.Core.Managers.PlacementManager.Instance.GetRequiredItemId(slotId);
            if (!string.IsNullOrEmpty(requiredItemId) && coreRequiredId != requiredItemId)
                Debug.LogError($"FoodSlot '{name}' cấu hình lệch Core: {requiredItemId} != {coreRequiredId}");

            // Kiểm tra trạng thái đã hoàn thành khi nạp game
            if (JourneyOfMemories.Core.Managers.PlacementManager.Instance.IsSlotFilled(slotId))
            {
                isCompleted = true;
                RestoreVisual(JourneyOfMemories.Core.Managers.PlacementManager.Instance.GetItemInSlot(slotId));
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
            var result = JourneyOfMemories.Core.Managers.PlacementManager.Instance.TryPlaceItem(food.ItemId, slotId);
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

        Debug.Log("Đặt đúng món: " + requiredFoodName);

        return true;
    }

    public bool TryPlaceItem(string itemId)
    {
        if (isCompleted || string.IsNullOrEmpty(itemId)) return false;
        var placement = JourneyOfMemories.Core.Managers.PlacementManager.Instance;
        if (placement == null) return false;
        if (placement.TryPlaceItem(itemId, slotId) != PlacementResult.Success) return false;

        isCompleted = true;
        RestoreVisual(itemId);
        return true;
    }

    private void RestoreVisual(string itemId)
    {
        FoodItem[] worldItems = FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var worldItem in worldItems)
        {
            if (worldItem != null && worldItem.ItemId == itemId)
            {
                worldItem.gameObject.SetActive(true);
                worldItem.transform.SetParent(transform);
                worldItem.transform.position = transform.position;
                worldItem.transform.localScale = Vector3.one * 0.5f;
                break;
            }
        }
    }
}
