using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MannequinController : L2_Interactable
{
    public enum MannequinSetType
    {
        Any = 0,
        Vietnam = 1,
        Korea = 2
    }

    [Header("Layered Slot Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;

    [Header("Target Configuration")]
    [SerializeField] private MannequinSetType targetSet = MannequinSetType.Any;

    [Header("Current Equipped Items")]
    [SerializeField] private string currentBodyID = "";
    [SerializeField] private string currentHeadID = "";

    [Header("Events")]
    [SerializeField] private UnityEvent onCombinationComplete = new UnityEvent();

    private bool isComplete = false;

    // Public properties
    public SpriteRenderer BodyRenderer => bodyRenderer;
    public SpriteRenderer HeadRenderer => headRenderer;
    public string CurrentBodyID => currentBodyID;
    public string CurrentHeadID => currentHeadID;
    public bool HasBodyItem => !string.IsNullOrEmpty(currentBodyID);
    public bool HasHeadItem => !string.IsNullOrEmpty(currentHeadID);
    public bool IsComplete => isComplete;
    public MannequinSetType TargetSet => targetSet;
    public UnityEvent OnCombinationComplete => onCombinationComplete ?? (onCombinationComplete = new UnityEvent());

    public bool IsVietnamSet => IsVietnamBody(currentBodyID) && IsVietnamHead(currentHeadID);
    public bool IsKoreaSet => IsKoreaBody(currentBodyID) && IsKoreaHead(currentHeadID);

    protected override void Awake()
    {
        base.Awake();

        // Auto-detect targetSet if not explicitly set
        if (targetSet == MannequinSetType.Any)
        {
            if (gameObject.name.Contains("AoDai") || gameObject.name.Contains("VietNam") || gameObject.name.Contains("NonLa"))
            {
                targetSet = MannequinSetType.Vietnam;
            }
            else if (gameObject.name.Contains("Hanbok") || gameObject.name.Contains("Korea") || gameObject.name.Contains("MuGat"))
            {
                targetSet = MannequinSetType.Korea;
            }
        }

        objectDisplayName = targetSet == MannequinSetType.Vietnam ? "MA NƠ CANH: VIỆT NAM" :
                            targetSet == MannequinSetType.Korea ? "MA NƠ CANH: HÀN QUỐC" : "MA NƠ CANH";

        // Initialize slots
        if (bodyRenderer != null && string.IsNullOrEmpty(currentBodyID) && bodyRenderer.sprite == null)
        {
            bodyRenderer.gameObject.SetActive(false);
        }
        if (headRenderer != null && string.IsNullOrEmpty(currentHeadID) && headRenderer.sprite == null)
        {
            headRenderer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Equips an item (shirt or hat) onto the corresponding slot renderer.
    /// </summary>
    public bool EquipItem(string itemID, Sprite itemSprite)
    {
        if (string.IsNullOrEmpty(itemID)) return false;

        string lower = itemID.Trim().ToLowerInvariant();

        if (IsBodyItem(lower))
        {
            currentBodyID = itemID;
            if (bodyRenderer != null)
            {
                bodyRenderer.gameObject.SetActive(true);
                bodyRenderer.enabled = true;
                if (itemSprite != null)
                {
                    bodyRenderer.sprite = itemSprite;
                    bodyRenderer.color = Color.white;
                }
            }
            CheckCombination();
            return true;
        }
        else if (IsHeadItem(lower))
        {
            currentHeadID = itemID;
            if (headRenderer != null)
            {
                headRenderer.gameObject.SetActive(true);
                headRenderer.enabled = true;
                if (itemSprite != null)
                {
                    headRenderer.sprite = itemSprite;
                    headRenderer.color = Color.white;
                }
            }
            CheckCombination();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether the mannequin has a valid full costume combination.
    /// (Áo dài + Nón lá HOẶC Hanbok + Mũ Gat)
    /// </summary>
    public bool CheckCombination()
    {
        bool isVnSet = IsVietnamBody(currentBodyID) && IsVietnamHead(currentHeadID);
        bool isKrSet = IsKoreaBody(currentBodyID) && IsKoreaHead(currentHeadID);

        bool matched = false;
        if (targetSet == MannequinSetType.Vietnam)
        {
            matched = isVnSet;
        }
        else if (targetSet == MannequinSetType.Korea)
        {
            matched = isKrSet;
        }
        else // Any
        {
            matched = isVnSet || isKrSet;
        }

        if (matched && !isComplete)
        {
            isComplete = true;
            onCombinationComplete?.Invoke();

            if (L2_GameManager.Instance != null)
            {
                if (isVnSet)
                {
                    L2_GameManager.Instance.OnItemPlacedOnMannequin(L2_ItemType.AoDai);
                    L2_GameManager.Instance.OnItemPlacedOnMannequin(L2_ItemType.NonLa);
                    L2_GameManager.Instance.SetQuestHint("Đã hoàn thành trưng bày Ma nơ canh Việt Nam (Áo dài & Nón lá)!");
                }
                else if (isKrSet)
                {
                    L2_GameManager.Instance.OnItemPlacedOnMannequin(L2_ItemType.Hanbok);
                    L2_GameManager.Instance.OnItemPlacedOnMannequin(L2_ItemType.MuGat);
                    L2_GameManager.Instance.SetQuestHint("Đã hoàn thành trưng bày Ma nơ canh Hàn Quốc (Hanbok & Mũ Gat)!");
                }
            }

            return true;
        }

        return isComplete;
    }

    /// <summary>
    /// Interaction entry point: checks player inventory and places matching item(s).
    /// </summary>
    public override void Interact()
    {
        if (isComplete)
        {
            if (L2_GameManager.Instance != null)
            {
                string setName = IsVietnamSet ? "Ma nơ canh Việt Nam (Áo dài & Nón lá)" : "Ma nơ canh Hàn Quốc (Hanbok & Mũ Gat)";
                L2_GameManager.Instance.SetQuestHint($"{setName} đã được trưng bày hoàn tất!");
            }
            return;
        }

        if (L2_InventoryManager.Instance == null) return;

        bool isVnTarget = targetSet == MannequinSetType.Vietnam ||
            (targetSet == MannequinSetType.Any && (IsVietnamBody(currentBodyID) || IsVietnamHead(currentHeadID) || gameObject.name.Contains("AoDai") || gameObject.name.Contains("VietNam")));

        bool isKrTarget = targetSet == MannequinSetType.Korea ||
            (targetSet == MannequinSetType.Any && (IsKoreaBody(currentBodyID) || IsKoreaHead(currentHeadID) || gameObject.name.Contains("Hanbok") || gameObject.name.Contains("Korea")));

        L2_ItemType bodyType = isKrTarget ? L2_ItemType.Hanbok : L2_ItemType.AoDai;
        L2_ItemType headType = isKrTarget ? L2_ItemType.MuGat : L2_ItemType.NonLa;
        string bodyName = isKrTarget ? "Hanbok Hàn Quốc" : "Áo dài Việt Nam";
        string headName = isKrTarget ? "Mũ Gat Hàn Quốc" : "Nón lá Việt Nam";

        // Dynamic detection for Any if no prior placement or name match
        if (targetSet == MannequinSetType.Any && !isVnTarget && !isKrTarget)
        {
            if (L2_InventoryManager.Instance.HasItem(L2_ItemType.AoDai) || L2_InventoryManager.Instance.HasItem(L2_ItemType.NonLa))
            {
                bodyType = L2_ItemType.AoDai;
                headType = L2_ItemType.NonLa;
                bodyName = "Áo dài Việt Nam";
                headName = "Nón lá Việt Nam";
            }
            else if (L2_InventoryManager.Instance.HasItem(L2_ItemType.Hanbok) || L2_InventoryManager.Instance.HasItem(L2_ItemType.MuGat))
            {
                bodyType = L2_ItemType.Hanbok;
                headType = L2_ItemType.MuGat;
                bodyName = "Hanbok Hàn Quốc";
                headName = "Mũ Gat Hàn Quốc";
            }
        }

        bool canPlaceBody = !HasBodyItem && L2_InventoryManager.Instance.HasItem(bodyType);
        bool canPlaceHead = !HasHeadItem && L2_InventoryManager.Instance.HasItem(headType);

        if (!canPlaceBody && !canPlaceHead)
        {
            if (L2_GameManager.Instance != null)
            {
                if (!HasBodyItem && !HasHeadItem)
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {bodyName} hoặc {headName} trước!");
                else if (!HasBodyItem)
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {bodyName} để hoàn thiện bộ trang phục!");
                else
                    L2_GameManager.Instance.SetQuestHint($"Bạn cần tìm {headName} để hoàn thiện bộ trang phục!");
            }
            return;
        }

        List<string> placedNames = new List<string>();

        if (canPlaceBody)
        {
            Sprite sprite = L2_InventoryManager.Instance.GetItemSprite(bodyType);
            EquipItem(bodyType.ToItemId(), sprite);
            L2_InventoryManager.Instance.RemoveItem(bodyType);
            placedNames.Add(bodyName);
        }

        if (canPlaceHead)
        {
            Sprite sprite = L2_InventoryManager.Instance.GetItemSprite(headType);
            EquipItem(headType.ToItemId(), sprite);
            L2_InventoryManager.Instance.RemoveItem(headType);
            placedNames.Add(headName);
        }

        CheckCombination();

        if (!isComplete && L2_GameManager.Instance != null)
        {
            string placedStr = string.Join(" và ", placedNames);
            string missingStr = !HasBodyItem ? bodyName : headName;
            L2_GameManager.Instance.SetQuestHint($"Đã đặt {placedStr}! Hãy tìm thêm {missingStr}.");
        }
    }

    // Helper methods for Inspector/Testing
    public void SetRenderers(SpriteRenderer body, SpriteRenderer head)
    {
        bodyRenderer = body;
        headRenderer = head;
    }

    public void SetTargetSet(MannequinSetType setType)
    {
        targetSet = setType;
    }

    public static bool IsBodyItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("aodai") || lower.Contains("ao_dai") || lower.Contains("áo dài") || lower.Contains("hanbok");
    }

    public static bool IsHeadItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("nonla") || lower.Contains("non_la") || lower.Contains("nón lá") || lower.Contains("mugat") || lower.Contains("mu_gat") || lower.Contains("mũ gat");
    }

    public static bool IsVietnamBody(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("aodai") || lower.Contains("ao_dai") || lower.Contains("áo dài");
    }

    public static bool IsVietnamHead(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("nonla") || lower.Contains("non_la") || lower.Contains("nón lá");
    }

    public static bool IsKoreaBody(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("hanbok");
    }

    public static bool IsKoreaHead(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        string lower = id.ToLowerInvariant();
        return lower.Contains("mugat") || lower.Contains("mu_gat") || lower.Contains("mũ gat");
    }
}
