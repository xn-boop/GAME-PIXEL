using UnityEngine;

public class L1_ItemPickup : L1_Interactable
{
    [Header("Item Configuration")]
    [SerializeField] private L1_ItemType itemType;
    [SerializeField] private SpriteRenderer visualSprite;
    [SerializeField] private GameObject arrowIndicator;

    private bool isUnlocked = false;
    private bool isCollected = false;

    public L1_ItemType ItemType
    {
        get
        {
            if (gameObject != null)
            {
                if (gameObject.name.Contains("Kimchi")) return L1_ItemType.Kimchi;
                if (gameObject.name.Contains("Kimbap")) return L1_ItemType.Kimbap;
                if (gameObject.name.Contains("ThitKho")) return L1_ItemType.ThitKho;
            }
            return itemType;
        }
    }
    public bool IsUnlocked => isUnlocked;
    public bool IsCollected => isCollected;
    public override bool CanInteract => !isCollected;

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
            }
            return arrowIndicator;
        }
    }

    private void OnValidate()
    {
        if (gameObject.name.Contains("ThitKho"))
        {
            itemType = L1_ItemType.ThitKho;
            objectDisplayName = "THỊT KHO TRỨNG";
        }
        else if (gameObject.name.Contains("Kimchi"))
        {
            itemType = L1_ItemType.Kimchi;
            objectDisplayName = "KIM CHI";
        }
        else if (gameObject.name.Contains("Kimbap"))
        {
            itemType = L1_ItemType.Kimbap;
            objectDisplayName = "KIMBAP";
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (gameObject.name.Contains("ThitKho"))
        {
            itemType = L1_ItemType.ThitKho;
            objectDisplayName = "THỊT KHO TRỨNG";
        }
        else if (gameObject.name.Contains("Kimchi"))
        {
            itemType = L1_ItemType.Kimchi;
            objectDisplayName = "KIM CHI";
        }
        else if (gameObject.name.Contains("Kimbap"))
        {
            itemType = L1_ItemType.Kimbap;
            objectDisplayName = "KIMBAP";
        }

        if (visualSprite == null)
        {
            Transform visual = transform.Find("Visual_Sprite");
            if (visual != null)
            {
                visualSprite = visual.GetComponent<SpriteRenderer>();
            }
        }

        if (arrowIndicator == null)
        {
            Transform arrow = transform.Find("Arrow_Indicator");
            if (arrow != null)
            {
                arrowIndicator = arrow.gameObject;
            }
        }

        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }
    }

    public void UnlockPickup()
    {
        isUnlocked = true;
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
        if (isCollected) return;

        if (!isUnlocked)
        {
            if (L1_GameManager.Instance != null)
            {
                L1_GameManager.Instance.SetQuestHint("Món ăn chưa thể thu thập, hãy hỏi chú Mèo để tìm gợi ý!");
            }
            return;
        }

        L1_QuizData quiz = L1_QuizDatabase.GetQuizByItemType(itemType);

        if (L1_QuizManager.Instance != null && quiz != null)
        {
            L1_QuizManager.Instance.OpenQuiz(quiz, OnItemSolved);
        }
        else
        {
            OnItemSolved();
        }
    }

    private void OnItemSolved()
    {
        isCollected = true;

        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }

        if (L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.OnItemCollected(itemType);
        }

        // Hide visual or deactivate
        gameObject.SetActive(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }
    }
}
