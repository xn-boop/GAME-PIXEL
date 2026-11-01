using UnityEngine;

namespace Game.Map3
{
    public class Map3_CameraController : MonoBehaviour
    {
        public static Map3_CameraController Instance { get; private set; }

        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Bounds Settings")]
        [SerializeField] private BoxCollider2D currentBounds;

        private Camera cam;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (target == null && Map3_PlayerController.Instance != null)
            {
                target = Map3_PlayerController.Instance.transform;
            }
        }

        public void SetBounds(BoxCollider2D newBounds, bool snapImmediate = false)
        {
            currentBounds = newBounds;
            if (snapImmediate && target != null)
            {
                Vector3 desired = GetClampedPosition(target.position + offset);
                transform.position = desired;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (Map3_PlayerController.Instance != null)
                {
                    target = Map3_PlayerController.Instance.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 desiredPosition = target.position + offset;

            if (currentBounds != null)
            {
                desiredPosition = GetClampedPosition(desiredPosition);
            }

            desiredPosition.z = offset.z;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        private Vector3 GetClampedPosition(Vector3 rawPosition)
        {
            if (currentBounds == null) return rawPosition;

            Bounds b = currentBounds.bounds;
            if (cam == null) cam = GetComponent<Camera>();

            float camVertExtent = cam != null ? cam.orthographicSize : 5f;
            float camHorzExtent = cam != null ? cam.orthographicSize * cam.aspect : 5f * (16f / 9f);

            float minX = b.min.x + camHorzExtent;
            float maxX = b.max.x - camHorzExtent;
            float minY = b.min.y + camVertExtent;
            float maxY = b.max.y - camVertExtent;

            float clampedX = minX > maxX ? b.center.x : Mathf.Clamp(rawPosition.x, minX, maxX);
            float clampedY = minY > maxY ? b.center.y : Mathf.Clamp(rawPosition.y, minY, maxY);

            return new Vector3(clampedX, clampedY, rawPosition.z);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
