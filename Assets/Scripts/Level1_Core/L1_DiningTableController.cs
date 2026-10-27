using UnityEngine;

public class L1_DiningTableController : L1_Interactable
{
    [Header("Plates References")]
    [SerializeField] private L1_Plate plateThitKho;
    [SerializeField] private L1_Plate plateKimchi;
    [SerializeField] private L1_Plate plateKimbap;

    private int placedDishesCount = 0;
    public int PlacedDishesCount => placedDishesCount;
    public bool IsTableComplete => placedDishesCount >= 3;

    protected override void Awake()
    {
        base.Awake();
        objectDisplayName = "BÀN ĂN GIA ĐÌNH";

        if (plateThitKho == null)
        {
            plateThitKho = transform.Find("Plate_ThitKho")?.GetComponent<L1_Plate>()
                        ?? transform.Find("Plate_1")?.GetComponent<L1_Plate>();
        }

        if (plateKimchi == null)
        {
            plateKimchi = transform.Find("Plate_Kimchi")?.GetComponent<L1_Plate>()
                       ?? transform.Find("Plate_2")?.GetComponent<L1_Plate>();
        }

        if (plateKimbap == null)
        {
            plateKimbap = transform.Find("Plate_Kimbap")?.GetComponent<L1_Plate>()
                       ?? transform.Find("Plate_3")?.GetComponent<L1_Plate>();
        }
    }

    public void SetPlates(L1_Plate thitKho, L1_Plate kimchi, L1_Plate kimbap)
    {
        plateThitKho = thitKho;
        plateKimchi = kimchi;
        plateKimbap = kimbap;
    }

    public override void Interact()
    {
        if (IsTableComplete)
        {
            if (L1_GameManager.Instance != null)
            {
                if (L1_GameManager.Instance.IsVictory)
                {
                    L1_GameManager.Instance.SetQuestHint("Bữa cơm gia đình và nét ẩm thực Việt - Hàn đã sẵn sàng!");
                }
                else
                {
                    L1_GameManager.Instance.SetQuestHint("Bàn ăn đã tươm tất! Hãy khám phá và giải đố thêm món Bánh mì và Tteokbokki trên bàn bếp để hoàn thành màn chơi.");
                }
            }
            return;
        }

        if (L1_InventoryManager.Instance == null) return;

        bool hasThitKho = L1_InventoryManager.Instance.HasItem(L1_ItemType.ThitKho);
        bool hasKimchi = L1_InventoryManager.Instance.HasItem(L1_ItemType.Kimchi);
        bool hasKimbap = L1_InventoryManager.Instance.HasItem(L1_ItemType.Kimbap);

        bool canPlaceThitKho = hasThitKho && (plateThitKho != null && !plateThitKho.IsPlaced);
        bool canPlaceKimchi = hasKimchi && (plateKimchi != null && !plateKimchi.IsPlaced);
        bool canPlaceKimbap = hasKimbap && (plateKimbap != null && !plateKimbap.IsPlaced);

        if (!canPlaceThitKho && !canPlaceKimchi && !canPlaceKimbap)
        {
            if (L1_GameManager.Instance != null)
            {
                if (placedDishesCount == 0)
                {
                    L1_GameManager.Instance.SetQuestHint("Bàn ăn cần 3 món: Thịt kho trứng, Kim chi, và Kimbap. Hãy tìm đủ món rồi quay lại nhé!");
                }
                else
                {
                    L1_GameManager.Instance.SetQuestHint($"Đã bày {placedDishesCount}/3 món lên bàn. Hãy tiếp tục tìm các món còn lại!");
                }
            }
            return;
        }

        // Place one available item per interaction
        if (canPlaceThitKho)
        {
            PlaceItem(L1_ItemType.ThitKho, plateThitKho);
        }
        else if (canPlaceKimchi)
        {
            PlaceItem(L1_ItemType.Kimchi, plateKimchi);
        }
        else if (canPlaceKimbap)
        {
            PlaceItem(L1_ItemType.Kimbap, plateKimbap);
        }
    }

    private void PlaceItem(L1_ItemType itemType, L1_Plate plate)
    {
        L1_InventoryManager.Instance.RemoveItem(itemType);
        Sprite foodSprite = L1_InventoryManager.Instance.GetItemSprite(itemType);
        if (plate != null)
        {
            plate.PlaceFood(foodSprite);
        }

        placedDishesCount++;

        string foodName = itemType.GetDisplayName();
        if (L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.OnFoodPlacedOnTable(itemType);
            if (placedDishesCount < 3)
            {
                L1_GameManager.Instance.SetQuestHint($"Đã bày món {foodName} lên đĩa! (Bàn ăn: {placedDishesCount}/3)");
            }
        }
    }
}
