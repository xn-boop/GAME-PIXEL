using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class L1_GameManager : MonoBehaviour
{
    public static L1_GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<L1_GameManager>();
            }
            return instance;
        }
        private set => instance = value;
    }
    private static L1_GameManager instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI txtObjective;
    [SerializeField] private TextMeshProUGUI txtQuestHint;
    [SerializeField] private GameObject panelVictory;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnContinue;

    [Header("Item Spawns (Main Foods)")]
    [SerializeField] private L1_ItemPickup thitKhoPickup;
    [SerializeField] private L1_ItemPickup kimchiPickup;
    [SerializeField] private L1_ItemPickup kimbapPickup;

    [Header("Arrow Indicators")]
    [SerializeField] private GameObject arrowThitKho;
    [SerializeField] private GameObject arrowKimchi;
    [SerializeField] private GameObject arrowKimbap;

    public GameObject ArrowThitKho => arrowThitKho;
    public GameObject ArrowKimchi => arrowKimchi;
    public GameObject ArrowKimbap => arrowKimbap;

    // Progression State
    private bool catQuizSolved = false;
    private int collectedCount = 0;
    private int placedCount = 0;
    private bool isBanhMiSolved = false;
    private bool isTteokbokkiSolved = false;
    private bool isVictory = false;

    public bool CatQuizSolved => catQuizSolved;
    public int CollectedCount => collectedCount;
    public int PlacedCount => placedCount;
    public bool IsBanhMiSolved => isBanhMiSolved;
    public bool IsTteokbokkiSolved => isTteokbokkiSolved;
    public bool IsVictory => isVictory;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Force hide hint arrows initially
        HideAllArrows();

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

    protected virtual void Start()
    {
        // Enforce hint arrows are hidden until Cat quiz is solved
        if (!catQuizSolved)
        {
            HideAllArrows();
        }

        UpdateObjectiveUI();
        SetQuestHint("Hãy tìm Chú Mèo để nhận gợi ý chuẩn bị bữa cơm gia đình.");
    }

    public void SetArrowReferences(GameObject thitKho, GameObject kimchi, GameObject kimbap)
    {
        arrowThitKho = thitKho;
        arrowKimchi = kimchi;
        arrowKimbap = kimbap;
    }

    public void HideAllArrows()
    {
        ResolveArrowReferences();
        if (arrowThitKho != null) arrowThitKho.SetActive(false);
        if (arrowKimchi != null) arrowKimchi.SetActive(false);
        if (arrowKimbap != null) arrowKimbap.SetActive(false);
    }

    public void ShowAllArrows()
    {
        ResolveArrowReferences();
        if (arrowThitKho != null) arrowThitKho.SetActive(true);
        if (arrowKimchi != null) arrowKimchi.SetActive(true);
        if (arrowKimbap != null) arrowKimbap.SetActive(true);
    }

    private void ResolveArrowReferences()
    {
        if (arrowThitKho == null)
        {
            var go = GameObject.Find("Interact_ThitKho");
            if (go != null)
            {
                var arrow = go.transform.Find("Arrow_Indicator");
                if (arrow != null) arrowThitKho = arrow.gameObject;
            }
        }
        if (arrowKimchi == null)
        {
            var go = GameObject.Find("Interact_Kimchi");
            if (go != null)
            {
                var arrow = go.transform.Find("Arrow_Indicator");
                if (arrow != null) arrowKimchi = arrow.gameObject;
            }
        }
        if (arrowKimbap == null)
        {
            var go = GameObject.Find("Interact_Kimbap");
            if (go != null)
            {
                var arrow = go.transform.Find("Arrow_Indicator");
                if (arrow != null) arrowKimbap = arrow.gameObject;
            }
        }
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
            int exploreCount = (isBanhMiSolved ? 1 : 0) + (isTteokbokkiSolved ? 1 : 0);
            txtObjective.text = $"MÓN ĂN: {collectedCount}/3 | BÀN ĂN: {placedCount}/3 | KHÁM PHÁ: {exploreCount}/2";
        }
    }

    public void OnCatQuizSolved()
    {
        if (catQuizSolved) return;
        catQuizSolved = true;

        // Hint arrows ONLY show after Cat Quiz is solved
        ShowAllArrows();

        if (thitKhoPickup != null) thitKhoPickup.UnlockPickup();
        if (kimchiPickup != null) kimchiPickup.UnlockPickup();
        if (kimbapPickup != null) kimbapPickup.UnlockPickup();

        var allPickups = UnityEngine.Object.FindObjectsByType<L1_ItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPickups)
        {
            if (p != null) p.UnlockPickup();
        }

        SetQuestHint("Chú mèo đã bật mí vị trí 3 món ăn! Hãy thu thập đủ Kim chi, Thịt kho trứng, Kimbap và bày lên 3 chiếc đĩa trên bàn ăn.");
        UpdateObjectiveUI();
    }

    public void OnItemCollected(L1_ItemType itemType)
    {
        collectedCount++;
        L1_InventoryManager.Instance?.AddItem(itemType);
        UpdateObjectiveUI();

        if (itemType == L1_ItemType.ThitKho && arrowThitKho != null) arrowThitKho.SetActive(false);
        else if (itemType == L1_ItemType.Kimchi && arrowKimchi != null) arrowKimchi.SetActive(false);
        else if (itemType == L1_ItemType.Kimbap && arrowKimbap != null) arrowKimbap.SetActive(false);

        L1_ItemPickup targetPickup = GetPickupByType(itemType);
        if (targetPickup != null)
        {
            targetPickup.SetArrowIndicatorActive(false);
        }

        SetQuestHint("Đã nhận món ăn thành công! Hãy tiếp tục khám phá.");
    }

    private L1_ItemPickup GetPickupByType(L1_ItemType itemType)
    {
        switch (itemType)
        {
            case L1_ItemType.ThitKho: return thitKhoPickup;
            case L1_ItemType.Kimchi: return kimchiPickup;
            case L1_ItemType.Kimbap: return kimbapPickup;
            default: return null;
        }
    }

    public void OnFoodPlacedOnTable(L1_ItemType itemType)
    {
        placedCount++;
        UpdateObjectiveUI();
        CheckWinCondition();
    }

    public void OnCulturalItemSolved(L1_CulturalType type)
    {
        if (type == L1_CulturalType.BanhMi)
        {
            isBanhMiSolved = true;
        }
        else if (type == L1_CulturalType.Tteokbokki)
        {
            isTteokbokkiSolved = true;
        }

        UpdateObjectiveUI();

        if (type == L1_CulturalType.BanhMi || type == L1_CulturalType.Tteokbokki)
        {
            CheckWinCondition();

            if (!isVictory && !(isBanhMiSolved && isTteokbokkiSolved))
            {
                string solvedName = type == L1_CulturalType.BanhMi ? "Bánh mì" : "Tteokbokki";
                SetQuestHint($"Đã giải mã xong {solvedName}! Hãy tiếp tục khám phá món còn lại trên bàn bếp.");
            }
        }
        else
        {
            SetQuestHint("Đã khám phá nét đẹp văn hóa thành công! Hãy tiếp tục hành trình.");
        }
    }

    public void CheckWinCondition()
    {
        if (isVictory) return;

        bool isDishesComplete = placedCount >= 3;
        bool isExploreComplete = isBanhMiSolved && isTteokbokkiSolved;

        if (isDishesComplete && isExploreComplete)
        {
            isVictory = true;
            SetQuestHint("Bữa cơm gia đình và nét ẩm thực Việt - Hàn đã sẵn sàng! Bạn đã hoàn thành Màn 1!");
            StartCoroutine(VictorySequenceRoutine());
        }
        else if (isDishesComplete && !isExploreComplete)
        {
            SetQuestHint("Bàn ăn đã tươm tất! Hãy khám phá và giải đố thêm món Bánh mì và Tteokbokki trên bàn bếp để hoàn thành màn chơi.");
        }
        else if (isExploreComplete && !isDishesComplete)
        {
            SetQuestHint("Đã giải mã xong Bánh mì và Tteokbokki! Hãy thu thập và bày đủ 3 món ăn chính lên bàn ăn để qua màn.");
        }
    }

    private IEnumerator VictorySequenceRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        if (panelVictory != null)
        {
            panelVictory.SetActive(true);
        }

        if (L1_PlayerController.Instance != null)
        {
            L1_PlayerController.Instance.SetMovementLocked(true);
        }
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ContinueGame()
    {
        // Proceed to Map 2
        if (Application.CanStreamedLevelBeLoaded("Scene_Level2"))
        {
            SceneManager.LoadScene("Scene_Level2");
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
