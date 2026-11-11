using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class L2_GameManager : MonoBehaviour
{
    public static L2_GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI txtObjective;
    [SerializeField] private TextMeshProUGUI txtQuestHint;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnContinue;

    [Header("Item Spawns")]
    [SerializeField] private L2_ItemPickup aoDaiPickup;
    [SerializeField] private L2_ItemPickup hanbokPickup;
    [SerializeField] private L2_ItemPickup nonLaPickup;
    [SerializeField] private L2_ItemPickup muGatPickup;

    // Progression State
    private bool catQuizSolved = false;
    private int paintingsSolvedCount = 0;
    private int collectedCount = 0;
    private int placedCount = 0;

    // Item Placed Flags (4 items total across 2 Mannequins)
    private bool isAoDaiPlaced = false;
    private bool isNonLaPlaced = false;
    private bool isHanbokPlaced = false;
    private bool isMuGatPlaced = false;
    private bool isVictory = false;

    public bool CatQuizSolved => catQuizSolved;
    public int CollectedCount => collectedCount;
    public int PlacedCount => placedCount;
    public bool IsVictory => isVictory;

    // Mannequin states
    public bool IsMannequinVnComplete => isAoDaiPlaced && isNonLaPlaced;
    public bool IsMannequinKrComplete => isHanbokPlaced && isMuGatPlaced;
    public bool AllMannequinsComplete => IsMannequinVnComplete && IsMannequinKrComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panelVictory != null)
        {
            panelVictory.SetActive(false);
        }

        if (btnRestart != null)
        {
            btnRestart.onClick.AddListener(RestartGame);
        }

        if (btnContinue != null)
        {
            btnContinue.onClick.AddListener(ContinueGame);
        }
    }

    private void Start()
    {
        UpdateObjectiveUI();
        SetQuestHint("Tương tác với chú mèo để tìm gợi ý...");
    }

    public void SetQuestHint(string text)
    {
        if (txtQuestHint != null)
        {
            txtQuestHint.text = text;
        }
    }

    public void UpdateObjectiveUI()
    {
        if (txtObjective != null)
        {
            if (placedCount >= 4)
            {
                txtObjective.text = "TRANG PHỤC: 4/4";
            }
            else
            {
                txtObjective.text = $"TRANG PHỤC: {collectedCount}/4 (Đã nhặt: {collectedCount})";
            }
        }
    }

    public void OnCatQuizSolved()
    {
        if (catQuizSolved) return;
        catQuizSolved = true;

        if (aoDaiPickup != null) aoDaiPickup.UnlockPickup();
        if (hanbokPickup != null) hanbokPickup.UnlockPickup();
        if (nonLaPickup != null) nonLaPickup.UnlockPickup();
        if (muGatPickup != null) muGatPickup.UnlockPickup();

        // Also search for any L2_ItemPickup in scene in case fields weren't wired in Inspector
        var allPickups = Object.FindObjectsByType<L2_ItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPickups)
        {
            if (p != null) p.UnlockPickup();
        }

        SetQuestHint("Đi theo các mũi tên chỉ dẫn để thu thập Áo dài, Nón lá, Hanbok, Mũ Gat!");
    }

    public void OnPaintingSolved(int index)
    {
        paintingsSolvedCount++;
        SetQuestHint("Đã giải mã tranh thành công! Hãy tiếp tục khám phá.");
    }

    public void OnItemCollected(L2_ItemType itemType)
    {
        collectedCount++;
        L2_InventoryManager.Instance?.AddItem(itemType);
        UpdateObjectiveUI();

        // Deactivate arrow indicator for this item if tracked
        L2_ItemPickup targetPickup = GetPickupByType(itemType);
        if (targetPickup != null)
        {
            targetPickup.SetArrowIndicatorActive(false);
        }

        string name = GetItemDisplayName(itemType);
        SetQuestHint($"Đã thu thập {name}! Hãy mang đến phòng Ma nơ canh.");
    }

    private L2_ItemPickup GetPickupByType(L2_ItemType itemType)
    {
        switch (itemType)
        {
            case L2_ItemType.AoDai: return aoDaiPickup;
            case L2_ItemType.NonLa: return nonLaPickup;
            case L2_ItemType.Hanbok: return hanbokPickup;
            case L2_ItemType.MuGat: return muGatPickup;
            default: return null;
        }
    }

    public void OnItemPlacedOnMannequin(L2_ItemType itemType)
    {
        if (itemType == L2_ItemType.AoDai) isAoDaiPlaced = true;
        else if (itemType == L2_ItemType.NonLa) isNonLaPlaced = true;
        else if (itemType == L2_ItemType.Hanbok) isHanbokPlaced = true;
        else if (itemType == L2_ItemType.MuGat) isMuGatPlaced = true;

        placedCount = (isAoDaiPlaced ? 1 : 0) + (isNonLaPlaced ? 1 : 0) + (isHanbokPlaced ? 1 : 0) + (isMuGatPlaced ? 1 : 0);
        UpdateObjectiveUI();

        CheckVictoryCondition();
    }

    public void OnMannequinPlaced(L2_ItemType itemType)
    {
        OnItemPlacedOnMannequin(itemType);
    }

    private void CheckVictoryCondition()
    {
        // Victory condition: ONLY activate victory when BOTH mannequins are completely dressed
        if (IsMannequinVnComplete && IsMannequinKrComplete && !isVictory)
        {
            isVictory = true;
            UpdateObjectiveUI(); // Displays "TRANG PHỤC: 4/4"
            StartCoroutine(VictorySequenceRoutine());
        }
    }

    private string GetItemDisplayName(L2_ItemType itemType)
    {
        switch (itemType)
        {
            case L2_ItemType.AoDai: return "Áo dài Việt Nam";
            case L2_ItemType.NonLa: return "Nón lá Việt Nam";
            case L2_ItemType.Hanbok: return "Hanbok Hàn Quốc";
            case L2_ItemType.MuGat: return "Mũ Gat Hàn Quốc";
            default: return "Vật phẩm";
        }
    }

    private IEnumerator VictorySequenceRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (panelVictory != null)
        {
            panelVictory.SetActive(true);
        }

        if (L2_PlayerController.Instance != null)
        {
            L2_PlayerController.Instance.SetMovementLocked(true);
        }

        // Tự động chuyển sang màn hình Puzzle sau 5 giây nếu người chơi chưa bấm nút Tiếp tục
        yield return new WaitForSeconds(5.0f);
        if (isVictory && panelVictory != null && panelVictory.activeSelf)
        {
            ContinueGame();
        }
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ContinueGame()
    {
        // Ghi nhận hoàn thành Màn 2 để mở khóa Mảnh ghép 2 trong scene "Puzzle"
        PlayerPrefs.SetInt("Puzzle_RecentCompletedMap", 2);
        PlayerPrefs.SetInt("Puzzle_UnlockedPieces", Mathf.Max(PlayerPrefs.GetInt("Puzzle_UnlockedPieces", 0), 2));
        PlayerPrefs.Save();

        // Chuyển sang scene "Puzzle" để hiển thị hiệu ứng xuất hiện dần mảnh ảnh 2
        if (Application.CanStreamedLevelBeLoaded("Puzzle"))
        {
            SceneManager.LoadScene("Puzzle");
        }
        else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Puzzle.unity"))
        {
            SceneManager.LoadScene("Assets/Scenes/Puzzle.unity");
        }
        else if (Application.CanStreamedLevelBeLoaded("Puzzel"))
        {
            SceneManager.LoadScene("Puzzel");
        }
        // Proceed to Map 3
        else if (Application.CanStreamedLevelBeLoaded("Map3_Final"))
        {
            SceneManager.LoadScene("Map3_Final");
        }
        else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Scenes/Map3_Final.unity"))
        {
            SceneManager.LoadScene("Assets/Scenes/Scenes/Map3_Final.unity");
        }
        else if (Application.CanStreamedLevelBeLoaded("Assets/_Map3_Final/Scenes/Map3_Final.unity"))
        {
            SceneManager.LoadScene("Assets/_Map3_Final/Scenes/Map3_Final.unity");
        }
        else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            RestartGame();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
