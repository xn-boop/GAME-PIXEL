using UnityEngine;

public class L2_CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-4f, -2.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(4f, 2.5f);

    private void Start()
    {
        if (target == null && L2_PlayerController.Instance != null)
        {
            target = L2_PlayerController.Instance.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (L2_PlayerController.Instance != null)
            {
                target = L2_PlayerController.Instance.transform;
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
