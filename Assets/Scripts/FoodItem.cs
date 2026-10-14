
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [Header("Food Information")]
    [SerializeField] private string foodName = "Food";
    [SerializeField] private Sprite inventoryIcon;

    [SerializeField] private string quizId = "";

    private bool isCollected = false;

    public string FoodName => foodName;
    public string QuizId => quizId;
    public bool IsCollected => isCollected;
    public Sprite InventoryIcon => inventoryIcon;

    private void Start()
    {
        // Chống nhân đôi: Nếu vật phẩm đã có trong túi hoặc đã đặt lên slot
        if (JourneyOfMemories.Core.Managers.SaveLoadManager.Instance != null &&
            JourneyOfMemories.Core.Managers.SaveLoadManager.Instance.IsItemAlreadyCollectedOrPlaced(foodName))
        {
            gameObject.SetActive(false);
        }
    }

    public void Collect(Transform player)
    {
        if (isCollected) return;

        isCollected = true;
        transform.SetParent(player);
        transform.localPosition = new Vector3(0f, 0.7f, 0f);

        Collider2D itemCollider = GetComponent<Collider2D>();

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        Debug.Log("Đã nhặt: " + foodName);
    }

    public void ResetCollection()
    {
        isCollected = false;
    }
}
