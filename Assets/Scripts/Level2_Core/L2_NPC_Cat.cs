using UnityEngine;

public class L2_NPC_Cat : L2_Interactable
{
    [Header("Item Arrow Indicators (Optional Inspector Assignment)")]
    [SerializeField] private GameObject[] arrowIndicators;

    private bool isCatSolved = false;

    public bool IsCatSolved => isCatSolved;

    protected override void Awake()
    {
        base.Awake();
        objectDisplayName = "MÈO";
    }

    public override void Interact()
    {
        if (isCatSolved)
        {
            if (L2_GameManager.Instance != null)
            {
                L2_GameManager.Instance.SetQuestHint("Mèo: Mau tìm Áo dài, Nón lá, Hanbok và Mũ Gat theo hướng mũi tên chỉ dẫn nhé!");
            }
            return;
        }

        if (L2_QuizManager.Instance != null)
        {
            L2_QuizManager.Instance.OpenQuiz(L2_QuizDatabase.CatQuiz, OnCatSolved);
        }
    }

    private void OnCatSolved()
    {
        isCatSolved = true;

        // Activate any directly referenced arrow indicators
        if (arrowIndicators != null)
        {
            foreach (var arrow in arrowIndicators)
            {
                if (arrow != null)
                {
                    arrow.SetActive(true);
                }
            }
        }

        // Notify GameManager to unlock all item pickups and their arrow indicators
        if (L2_GameManager.Instance != null)
        {
            L2_GameManager.Instance.OnCatQuizSolved();
        }
    }
}
