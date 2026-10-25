using UnityEngine;

public class L2_ItemPickup : L2_Interactable
{
    [Header("Item Configuration")]
    [SerializeField] private L2_ItemType itemType;
    [SerializeField] private SpriteRenderer visualSprite;
    [SerializeField] private GameObject arrowIndicator;

    private bool isUnlocked = false;

    public L2_ItemType ItemType => itemType;
    public bool IsUnlocked => isUnlocked;
    public GameObject ArrowIndicator
    {
        get
        {
            if (arrowIndicator == null)
            {
                Transform arrow = transform.Find("Arrow_Indicator");
                if (arrow != null)
                {
                    arrowIndicator = arrow.gameObject;
                }
                else
                {
                    arrowIndicator = CreateDefaultArrowIndicator();
                    if (arrowIndicator != null)
                    {
                        arrowIndicator.SetActive(isUnlocked);
                    }
                }
            }
            return arrowIndicator;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // Auto-assign itemType based on GameObject name to guarantee proper item association
        if (gameObject.name.Contains("NonLa"))
        {
            itemType = L2_ItemType.NonLa;
            objectDisplayName = "NÓN LÁ";
        }
        else if (gameObject.name.Contains("MuGat"))
        {
            itemType = L2_ItemType.MuGat;
            objectDisplayName = "MŨ GAT";
        }
        else if (gameObject.name.Contains("AoDai"))
        {
            itemType = L2_ItemType.AoDai;
            objectDisplayName = "ÁO DÀI";
        }
        else if (gameObject.name.Contains("Hanbok"))
        {
            itemType = L2_ItemType.Hanbok;
            objectDisplayName = "HANBOK";
        }

        if (visualSprite == null)
        {
            Transform visual = transform.Find("Visual_Sprite");
            if (visual != null)
            {
                visualSprite = visual.GetComponent<SpriteRenderer>();
            }
        }

        // Find or reference Arrow_Indicator
        if (arrowIndicator == null)
        {
            Transform arrow = transform.Find("Arrow_Indicator");
            if (arrow != null)
            {
                arrowIndicator = arrow.gameObject;
            }
        }

        // Create fallback Arrow_Indicator placeholder if none provided by Art Integrator yet
        if (arrowIndicator == null)
        {
            arrowIndicator = CreateDefaultArrowIndicator();
        }

        // Ensure arrow indicator is initially inactive
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }
    }

    private void Start()
    {
        // Replaced old yellow highlight/dimming mechanism with Arrow Indicator
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(isUnlocked);
        }
    }

    public void UnlockPickup()
    {
        isUnlocked = true;
        // Activate Arrow Indicator pointing to this item
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(true);
        }
    }

    public void SetArrowIndicatorActive(bool active)
    {
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(active);
        }
    }

    public override void Interact()
    {
        if (!isUnlocked)
        {
            if (L2_GameManager.Instance != null)
            {
                L2_GameManager.Instance.SetQuestHint("Món đồ chưa thể thu thập, hãy hỏi chú Mèo để tìm gợi ý!");
            }
            return;
        }

        L2_QuizData quiz = GetQuizForItem();

        if (L2_QuizManager.Instance != null && quiz != null)
        {
            L2_QuizManager.Instance.OpenQuiz(quiz, OnItemSolved);
        }
        else
        {
            OnItemSolved();
        }
    }

    private L2_QuizData GetQuizForItem()
    {
        return L2_QuizDatabase.GetQuizByItemType(itemType);
    }

    private void OnItemSolved()
    {
        // Hide arrow indicator immediately upon successful pickup
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }

        if (L2_GameManager.Instance != null)
        {
            L2_GameManager.Instance.OnItemCollected(itemType);
        }

        gameObject.SetActive(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Hide arrow indicator when item object is deactivated
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }
    }

    private GameObject CreateDefaultArrowIndicator()
    {
        GameObject arrowGo = new GameObject("Arrow_Indicator");
        arrowGo.transform.SetParent(transform, false);
        arrowGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);

        var tmp = arrowGo.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "▼";
        tmp.fontSize = 4f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.76f, 0.03f, 1f); // Warm Gold / Amber
        tmp.sortingOrder = 15;

        arrowGo.AddComponent<L2_ArrowBobber>();

        return arrowGo;
    }
}

/// <summary>
/// Subtle floating / bobbing animation for arrow indicators in Level 2.
/// </summary>
public class L2_ArrowBobber : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 3.5f;
    [SerializeField] private float bobHeight = 0.2f;

    private Vector3 baseLocalPos;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeBasePos();
    }

    private void OnEnable()
    {
        InitializeBasePos();
        transform.localPosition = baseLocalPos;
    }

    private void InitializeBasePos()
    {
        if (!isInitialized)
        {
            baseLocalPos = transform.localPosition;
            isInitialized = true;
        }
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = baseLocalPos + new Vector3(0f, offset, 0f);
    }
}
