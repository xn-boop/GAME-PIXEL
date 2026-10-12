
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [Header("Food Information")]
    [SerializeField] private string foodName = "Food";
    [SerializeField] private Sprite inventoryIcon;

    private bool isCollected = false;

    public string FoodName => foodName;
    public bool IsCollected => isCollected;
    public Sprite InventoryIcon => inventoryIcon;

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
