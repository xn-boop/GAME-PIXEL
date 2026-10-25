using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

/// <summary>
/// Điều phối tiến trình và giao diện nhiệm vụ riêng của Map 1.
/// Bám sát brief & workflow Mâm cơm Việt - Hàn:
/// - Mèo là điểm hướng dẫn đầu tiên với dấu hỏi nổi.
/// - Trả lời đúng mèo mở gợi ý cho món đầu tiên.
/// - 5 món ăn có câu hỏi riêng và gợi ý tuần tự.
/// - Bộ đếm MÓN ĂN: x/5 cập nhật theo số món đã nhặt vào túi (bao gồm cả 2 món phụ).
/// - Bộ đếm ĐẶT MÂM: x/3 cập nhật theo số món chính đã đặt đúng vào đĩa.
/// - Sau khi đạt 5/5 và đặt đủ 3/3, kích hoạt hoàn thành mâm cơm.
/// </summary>
public class Map1WorkflowController : MonoBehaviour
{
    public static Map1WorkflowController Instance { get; private set; }

    [Header("UI Displays")]
    [Tooltip("Text đếm MÓN ĂN: 0/5")]
    [SerializeField] private TMP_Text foodCounterText;

    [Tooltip("Text chú thích (đã nhặt 0 món)")]
    [SerializeField] private TMP_Text foodSubtitleText;

    [Tooltip("Text đếm ĐẶT MÂM: 0/3")]
    [SerializeField] private TMP_Text dishPlacementText;

    [Tooltip("Khung thông báo gợi ý chính")]
    [SerializeField] private TMP_Text hintBannerText;

    [Header("Cat NPC")]
    [SerializeField] private CatInteraction catInteraction;

    [Header("Food Items in Scene")]
    [SerializeField] private FoodItem thitKhoItem;
    [SerializeField] private FoodItem kimchiItem;
    [SerializeField] private FoodItem comCuonItem;
    [SerializeField] private FoodItem banhMiItem;
    [SerializeField] private FoodItem tteokbokkiItem;

    [Header("Dining Table Slots")]
    [SerializeField] private FoodSlot slotThitKho;
    [SerializeField] private FoodSlot slotKimchi;
    [SerializeField] private FoodSlot slotComCuon;

    private readonly List<string> allFoodIds = new List<string>
    {
        GameIds.Item.ThitKhoTrung,
        GameIds.Item.Kimchi,
        GameIds.Item.ComCuon,
        GameIds.Item.BanhMi,
        GameIds.Item.Tteokbokki
    };

    private readonly List<string> mainDishSlotIds = new List<string>
    {
        GameIds.Slot.ThitKhoTrung,
        GameIds.Slot.Kimchi,
        GameIds.Slot.ComCuon
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.OnQuizSolved += OnQuizSolved;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated += OnInventoryUpdated;
        }
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnItemPlaced += OnItemPlaced;
        }
        if (catInteraction != null)
        {
            catInteraction.OnCatQuizCompleted += RefreshWorkflow;
        }
    }

    private void OnDisable()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.OnQuizSolved -= OnQuizSolved;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUpdated -= OnInventoryUpdated;
        }
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnItemPlaced -= OnItemPlaced;
        }
        if (catInteraction != null)
        {
            catInteraction.OnCatQuizCompleted -= RefreshWorkflow;
        }
    }

    private void Start()
    {
        RefreshWorkflow();
    }

    private void OnQuizSolved(string quizId, string rewardItemId)
    {
        // 1. Nếu là câu hỏi chú mèo
        if (quizId == GameIds.Quiz.CatIntro || quizId == "Q_Cat")
        {
            if (catInteraction != null)
            {
                catInteraction.SetCatSolved(true);
            }
        }

        // 2. Ẩn vật phẩm tương ứng trên thế giới nếu vừa nhận vào túi
        HideWorldItemIfMatches(rewardItemId);

        RefreshWorkflow();
    }

    private void OnInventoryUpdated(IReadOnlyList<string> items)
    {
        RefreshWorkflow();
    }

    private void OnItemPlaced(string slotId, string itemId)
    {
        RefreshWorkflow();
    }

    private void HideWorldItemIfMatches(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (thitKhoItem != null && thitKhoItem.ItemId == itemId) thitKhoItem.MarkCollectedAndHide();
        if (kimchiItem != null && kimchiItem.ItemId == itemId) kimchiItem.MarkCollectedAndHide();
        if (comCuonItem != null && comCuonItem.ItemId == itemId) comCuonItem.MarkCollectedAndHide();
        if (banhMiItem != null && banhMiItem.ItemId == itemId) banhMiItem.MarkCollectedAndHide();
        if (tteokbokkiItem != null && tteokbokkiItem.ItemId == itemId) tteokbokkiItem.MarkCollectedAndHide();
    }

    public void RefreshWorkflow()
    {
        int collectedCount = CountCollectedFoods();
        int placedCount = CountPlacedDishes();

        // 1. Cập nhật Text bộ đếm
        if (foodCounterText != null)
        {
            foodCounterText.text = $"MÓN ĂN: {collectedCount}/5";
        }

        if (foodSubtitleText != null)
        {
            foodSubtitleText.text = $"(đã nhặt {collectedCount} món)";
        }

        if (dishPlacementText != null)
        {
            dishPlacementText.text = $"ĐẶT MÂM: {placedCount}/3";
        }

        // 2. Xác định câu gợi ý theo trình tự logic brief
        bool isCatDone = (GameStateManager.Instance != null && GameStateManager.Instance.M1_MiniDone) ||
                         (catInteraction != null && catInteraction.IsSolved);

        string hint = GetCurrentHint(isCatDone, collectedCount, placedCount);

        if (hintBannerText != null)
        {
            hintBannerText.text = hint;
        }
    }

    private string GetCurrentHint(bool isCatDone, int collectedCount, int placedCount)
    {
        // Bước 1: Trước khi tương tác với mèo
        if (!isCatDone)
        {
            return "Hãy lại gần mèo xem gợi ý.";
        }

        // Bước 2: Tuần tự tìm 5 món theo brief
        if (!IsItemCollectedOrPlaced(GameIds.Item.ThitKhoTrung))
        {
            return "Hãy tìm chiếc nồi có nắp ở góc bếp.";
        }

        if (!IsItemCollectedOrPlaced(GameIds.Item.Kimchi))
        {
            return "Hãy tìm hũ thức ăn lên men gần tủ lạnh.";
        }

        if (!IsItemCollectedOrPlaced(GameIds.Item.ComCuon))
        {
            return "Hãy tìm đĩa kimbap trên bàn bếp.";
        }

        if (!IsItemCollectedOrPlaced(GameIds.Item.BanhMi))
        {
            return "Hãy tìm ổ bánh mì thịt trên kệ bếp.";
        }

        if (!IsItemCollectedOrPlaced(GameIds.Item.Tteokbokki))
        {
            return "Hãy tìm đĩa bánh gạo cay cạnh bồn rửa.";
        }

        // Bước 3: Đã tìm đủ 5 món -> mang 3 món chính tới bàn ăn
        if (placedCount < 3)
        {
            return "Bạn đã tìm đủ các món ăn. Hãy mang ba món chính tới bàn ăn.";
        }

        // Bước 4: Hoàn thành mâm cơm
        return "Mâm cơm gia đình Việt - Hàn đã hoàn tất ấm cúng!";
    }

    private bool IsItemCollectedOrPlaced(string itemId)
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(itemId))
            return true;

        if (PlacementManager.Instance != null)
        {
            foreach (var kvp in PlacementManager.Instance.FilledSlots)
            {
                if (kvp.Value == itemId) return true;
            }
        }

        return false;
    }

    public int CountCollectedFoods()
    {
        int count = 0;
        foreach (var id in allFoodIds)
        {
            if (IsItemCollectedOrPlaced(id))
            {
                count++;
            }
        }
        return count;
    }

    public int CountPlacedDishes()
    {
        if (PlacementManager.Instance == null) return 0;
        int count = 0;
        foreach (var slotId in mainDishSlotIds)
        {
            if (PlacementManager.Instance.IsSlotFilled(slotId))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Chuyển sang Map 2 (Phòng thay đồ) khi hoàn thành mâm cơm gia đình
    /// </summary>
    public void GoToMap2()
    {
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.RequestAndLoad(MapId.Map2_DressingRoom);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_Level2");
        }
    }
}
