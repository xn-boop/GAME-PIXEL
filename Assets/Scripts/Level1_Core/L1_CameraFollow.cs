using UnityEngine;

public class L1_CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-3.5f, -1.8f);
    [SerializeField] private Vector2 maxBounds = new Vector2(3.5f, 1.8f);

    private void Start()
    {
        if (target == null && L1_PlayerController.Instance != null)
        {
            target = L1_PlayerController.Instance.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (L1_PlayerController.Instance != null)
            {
                target = L1_PlayerController.Instance.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        desiredPosition.z = offset.z;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
