using UnityEngine;

public class L1_ArrowBobber : MonoBehaviour
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
