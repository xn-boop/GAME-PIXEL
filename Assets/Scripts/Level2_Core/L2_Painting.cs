using UnityEngine;

public class L2_Painting : L2_Interactable
{
    [Header("Painting Configuration")]
    [SerializeField] private int paintingIndex = 1;
    [SerializeField] private SpriteRenderer visualSprite;

    private bool isSolved = false;

    public int PaintingIndex => paintingIndex;
    public bool IsSolved => isSolved;

    private readonly Color solvedColor = new Color(0.298f, 0.686f, 0.314f, 1f); // Green #4CAF50

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
    }

    public override void Interact()
    {
        if (isSolved)
        {
            if (L2_GameManager.Instance != null)
            {
                L2_GameManager.Instance.SetQuestHint("Bức tranh này đã được khám phá!");
            }
            return;
        }

        if (L2_QuizManager.Instance != null)
        {
            L2_QuizManager.Instance.OpenQuiz(L2_QuizDatabase.GetPaintingQuiz(paintingIndex), OnPaintingSolved);
        }
    }

    private void OnPaintingSolved()
    {
        isSolved = true;
        if (visualSprite != null)
        {
            visualSprite.color = solvedColor;
        }

        if (L2_GameManager.Instance != null)
        {
            L2_GameManager.Instance.OnPaintingSolved(paintingIndex);
        }
    }
}
