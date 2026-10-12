
using UnityEngine;

public class FoodSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private string requiredFoodName;

    private bool isCompleted = false;

    public bool IsCompleted => isCompleted;

    public bool TryPlaceFood(FoodItem food)
    {
        // Kiểm tra món ăn hợp lệ
        if (food == null || isCompleted)
        {
            return false;
        }

        // Kiểm tra món ăn có đúng ô không
        if (food.FoodName != requiredFoodName)
        {
            Debug.Log("Sai món! Ô này cần: " + requiredFoodName);
            return false;
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
