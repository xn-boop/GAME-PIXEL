using UnityEngine;

public class L1_NPC_Cat : L1_Interactable
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
            if (L1_GameManager.Instance != null)
            {
                L1_GameManager.Instance.SetQuestHint("Mèo: Mau tìm Thịt kho trứng, Kim chi và Kimbap theo hướng mũi tên chỉ dẫn nhé!");
            }
            return;
        }

        if (L1_QuizManager.Instance != null)
        {
            L1_QuizManager.Instance.OpenQuiz(L1_QuizDatabase.CatQuiz, OnCatSolved);
        }
    }

    private void OnCatSolved()
    {
        isCatSolved = true;

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

        if (L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.OnCatQuizSolved();
        }
    }
}
