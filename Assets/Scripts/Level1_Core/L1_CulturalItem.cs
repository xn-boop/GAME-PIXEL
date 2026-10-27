using UnityEngine;

public enum L1_CulturalType
{
    FlagVN = 0,
    FlagKR = 1,
    BanhMi = 2,
    Tteokbokki = 3
}

public class L1_CulturalItem : L1_Interactable
{
    [Header("Cultural Item Settings")]
    [SerializeField] private L1_CulturalType culturalType;
    [SerializeField] private SpriteRenderer visualSprite;

    private bool isSolved = false;

    public L1_CulturalType CulturalType
    {
        get
        {
            if (gameObject != null)
            {
                if (gameObject.name.Contains("FlagVN")) return L1_CulturalType.FlagVN;
                if (gameObject.name.Contains("FlagKR")) return L1_CulturalType.FlagKR;
                if (gameObject.name.Contains("BanhMi")) return L1_CulturalType.BanhMi;
                if (gameObject.name.Contains("Tteokbokki")) return L1_CulturalType.Tteokbokki;
            }
            return culturalType;
        }
    }
    public bool IsSolved => isSolved;
    public override bool CanInteract => !isSolved;

    private readonly Color solvedColor = new Color(0.3f, 0.7f, 0.35f, 1f); // Green tint

    private void OnValidate()
    {
        if (gameObject.name.Contains("FlagVN"))
        {
            culturalType = L1_CulturalType.FlagVN;
            objectDisplayName = "QUỐC KỲ VIỆT NAM";
        }
        else if (gameObject.name.Contains("FlagKR"))
        {
            culturalType = L1_CulturalType.FlagKR;
            objectDisplayName = "QUỐC KỲ HÀN QUỐC";
        }
        else if (gameObject.name.Contains("BanhMi"))
        {
            culturalType = L1_CulturalType.BanhMi;
            objectDisplayName = "BÁNH MÌ VIỆT NAM";
        }
        else if (gameObject.name.Contains("Tteokbokki"))
        {
            culturalType = L1_CulturalType.Tteokbokki;
            objectDisplayName = "BÁNH GẠO CAY TTEOKBOKKI";
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (gameObject.name.Contains("FlagVN"))
        {
            culturalType = L1_CulturalType.FlagVN;
            objectDisplayName = "QUỐC KỲ VIỆT NAM";
        }
        else if (gameObject.name.Contains("FlagKR"))
        {
            culturalType = L1_CulturalType.FlagKR;
            objectDisplayName = "QUỐC KỲ HÀN QUỐC";
        }
        else if (gameObject.name.Contains("BanhMi"))
        {
            culturalType = L1_CulturalType.BanhMi;
            objectDisplayName = "BÁNH MÌ VIỆT NAM";
        }
        else if (gameObject.name.Contains("Tteokbokki"))
        {
            culturalType = L1_CulturalType.Tteokbokki;
            objectDisplayName = "BÁNH GẠO CAY TTEOKBOKKI";
        }

        if (visualSprite == null)
        {
            Transform visual = transform.Find("Visual_Sprite");
            if (visual != null)
            {
                visualSprite = visual.GetComponent<SpriteRenderer>();
            }
        }
    }

    public override void Interact()
    {
        if (isSolved)
        {
            if (L1_GameManager.Instance != null)
            {
                L1_GameManager.Instance.SetQuestHint("Nét đẹp văn hóa này đã được khám phá!");
            }
            return;
        }

        L1_QuizData quiz = GetQuiz();
        if (L1_QuizManager.Instance != null && quiz != null)
        {
            L1_QuizManager.Instance.OpenQuiz(quiz, OnQuizSolved);
        }
    }

    private L1_QuizData GetQuiz()
    {
        switch (culturalType)
        {
            case L1_CulturalType.FlagVN: return L1_QuizDatabase.FlagVnQuiz;
            case L1_CulturalType.FlagKR: return L1_QuizDatabase.FlagKrQuiz;
            case L1_CulturalType.BanhMi: return L1_QuizDatabase.BanhMiQuiz;
            case L1_CulturalType.Tteokbokki: return L1_QuizDatabase.TteokbokkiQuiz;
            default: return null;
        }
    }

    private void OnQuizSolved()
    {
        isSolved = true;
        ShowIndicator(false);

        if (L1_PlayerController.Instance != null)
        {
            L1_PlayerController.Instance.UnregisterInteractable(this);
        }

        if (visualSprite != null)
        {
            // Subtle highlight indicating solved
            visualSprite.color = Color.white;
        }

        if (L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.OnCulturalItemSolved(CulturalType);
        }
    }
}
