using UnityEngine;

public class L1_Plate : MonoBehaviour
{
    [Header("Plate Configuration")]
    [SerializeField] private L1_ItemType requiredItemType;
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private SpriteRenderer foodRenderer;

    private bool isPlaced = false;

    public L1_ItemType RequiredItemType => requiredItemType;
    public bool IsPlaced => isPlaced;

    private void Awake()
    {
        if (plateRenderer == null)
        {
            plateRenderer = GetComponent<SpriteRenderer>();
        }

        if (foodRenderer == null)
        {
            Transform f = transform.Find("Food_Sprite");
            if (f != null)
            {
                foodRenderer = f.GetComponent<SpriteRenderer>();
            }
        }

        if (foodRenderer != null)
        {
            foodRenderer.gameObject.SetActive(false);
            foodRenderer.sortingOrder = 6;
        }
    }

    public void PlaceFood(Sprite foodSprite = null)
    {
        isPlaced = true;
        if (foodRenderer != null)
        {
            if (foodSprite != null)
            {
                foodRenderer.sprite = foodSprite;
            }
            foodRenderer.gameObject.SetActive(true);
            foodRenderer.sortingOrder = 6;
        }
    }

    public void ResetPlate()
    {
        isPlaced = false;
        if (foodRenderer != null)
        {
            foodRenderer.gameObject.SetActive(false);
        }
    }
}
