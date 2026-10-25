using System.Collections.Generic;
using UnityEngine;

public class L2_Mannequin : L2_Interactable
{
    [Header("Mannequin Configuration")]
    [SerializeField] private L2_ItemType requiredItem;
    [SerializeField] private SpriteRenderer visualSprite;

    [Header("Visual Sprites (Art Integration Ready)")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite dressedSprite;

    [Header("Layered Mannequin Integration (Optional)")]
    [SerializeField] private MannequinController layeredController;

    private bool isPlaced = false;

    // Specific item placement state (each mannequin needs 2 items)
    private bool hasItem1 = false; // AoDai for VN, Hanbok for KR
    private bool hasItem2 = false; // NonLa for VN, MuGat for KR

    public L2_ItemType RequiredItem => requiredItem;
    public bool IsPlaced => isPlaced;
    public bool IsComplete => hasItem1 && hasItem2;
    public bool HasItem1 => hasItem1;
    public bool HasItem2 => hasItem2;

    public bool IsVietnamMannequin =>
        requiredItem == L2_ItemType.AoDai ||
        requiredItem == L2_ItemType.NonLa ||
        gameObject.name.Contains("AoDai") ||
        gameObject.name.Contains("VietNam");

    private readonly Color placedColor = new Color(0.95f, 0.12f, 0.22f, 1f); // Bright red
    private readonly Color partialColor = new Color(0.98f, 0.55f, 0.45f, 1f); // Semi-dressed tint

    protected override void Awake()
    {
        base.Awake();
        if (visualSprite == null)
        {
            Transform visual = transform.Find("Visual_Sprite");
            if (visual != null)
            {
                visualSprite = visual.GetComponent<SpriteRenderer>();
            }
        }

        if (emptySprite != null && visualSprite != null)
        {
            visualSprite.sprite = emptySprite;
        }

        if (layeredController == null)
        {
            layeredController = GetComponent<MannequinController>();
        }

        objectDisplayName = IsVietnamMannequin ? "MA NƠ CANH: VIỆT NAM" : "MA NƠ CANH: HÀN QUỐC";
    }

    public override void Interact()
    {
        if (IsComplete)
        {
            if (L2_GameManager.Instance != null)
            {
                string mName = IsVietnamMannequin ? "Ma nơ canh Việt Nam (Áo dài & Nón lá)" : "Ma nơ canh Hàn Quốc (Hanbok & Mũ Gat)";
                L2_GameManager.Instance.SetQuestHint($"{mName} đã được trưng bày hoàn tất!");
            }
            return;
        }

        if (L2_InventoryManager.Instance == null) return;

        L2_ItemType item1 = IsVietnamMannequin ? L2_ItemType.AoDai : L2_ItemType.Hanbok;
        L2_ItemType item2 = IsVietnamMannequin ? L2_ItemType.NonLa : L2_ItemType.MuGat;
        string name1 = IsVietnamMannequin ? "Áo dài Việt Nam" : "Hanbok Hàn Quốc";
        string name2 = IsVietnamMannequin ? "Nón lá Việt Nam" : "Mũ Gat Hàn Quốc";

        bool canPlace1 = !hasItem1 && L2_InventoryManager.Instance.HasItem(item1);
        bool canPlace2 = !hasItem2 && L2_InventoryManager.Instance.HasItem(item2);

        if (!canPlace1 && !canPlace2)
        {
            // Player has no applicable items for this mannequin
            if (L2_GameManager.Instance != null)
            {
                if (!hasItem1 && !hasItem2)
                {
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {name1} hoặc {name2} trước!");
                }
                else if (!hasItem1)
                {
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {name1} để hoàn thiện bộ trang phục!");
                }
                else
                {
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {name2} để hoàn thiện bộ trang phục!");
                }
            }
            return;
        }

        // Place applicable item(s) - either one or both if available
        List<string> placedNames = new List<string>();

        if (canPlace1)
        {
            hasItem1 = true;
            Sprite sprite1 = L2_InventoryManager.Instance.GetItemSprite(item1);
            if (layeredController != null)
            {
                layeredController.EquipItem(item1.ToItemId(), sprite1);
            }
            L2_InventoryManager.Instance.RemoveItem(item1);
            if (L2_GameManager.Instance != null)
            {
                L2_GameManager.Instance.OnItemPlacedOnMannequin(item1);
            }
            placedNames.Add(name1);
        }

        if (canPlace2)
        {
            hasItem2 = true;
            Sprite sprite2 = L2_InventoryManager.Instance.GetItemSprite(item2);
            if (layeredController != null)
            {
                layeredController.EquipItem(item2.ToItemId(), sprite2);
            }
            L2_InventoryManager.Instance.RemoveItem(item2);
            if (L2_GameManager.Instance != null)
            {
                L2_GameManager.Instance.OnItemPlacedOnMannequin(item2);
            }
            placedNames.Add(name2);
        }

        // Check completion state
        if (IsComplete)
        {
            isPlaced = true;
            if (visualSprite != null)
            {
                if (dressedSprite != null)
                {
                    visualSprite.sprite = dressedSprite;
                    visualSprite.color = Color.white;
                }
                else
                {
                    visualSprite.color = placedColor;
                }
            }

            if (L2_GameManager.Instance != null)
            {
                string mName = IsVietnamMannequin ? "Ma nơ canh Việt Nam (Áo dài & Nón lá)" : "Ma nơ canh Hàn Quốc (Hanbok & Mũ Gat)";
                L2_GameManager.Instance.SetQuestHint($"Đã hoàn thành trưng bày {mName}!");
            }
        }
        else
        {
            // Partially completed
            if (visualSprite != null && dressedSprite == null)
            {
                visualSprite.color = partialColor;
            }

            if (L2_GameManager.Instance != null)
            {
                string placedStr = string.Join(" và ", placedNames);
                string missingStr = !hasItem1 ? name1 : name2;
                L2_GameManager.Instance.SetQuestHint($"Đã đặt {placedStr}! Hãy tìm thêm {missingStr}.");
            }
        }
    }
}
