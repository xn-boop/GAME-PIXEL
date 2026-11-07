using UnityEngine;

namespace Game.Map0
{
    public class Map0_CameraController : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Camera Clamping")]
        [SerializeField] private BoxCollider2D cameraBounds;
        [SerializeField] private bool useBoundsClamping = true;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            if (useBoundsClamping && cameraBounds != null && cam != null)
            {
                float vertExtent = cam.orthographicSize;
                float horzExtent = vertExtent * cam.aspect;

                Bounds b = cameraBounds.bounds;

                // Clamp if camera view is smaller than bounds
                if (b.size.x >= horzExtent * 2f)
                {
                    float minX = b.min.x + horzExtent;
                    float maxX = b.max.x - horzExtent;
                    smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
                }
                else
                {
                    smoothedPosition.x = b.center.x;
                }

                if (b.size.y >= vertExtent * 2f)
                {
                    float minY = b.min.y + vertExtent;
                    float maxY = b.max.y - vertExtent;
                    smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
                }
                else
                {
                    smoothedPosition.y = b.center.y;
                }
            }

            smoothedPosition.z = offset.z;
            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(BoxCollider2D newBounds)
        {
            cameraBounds = newBounds;
        }
    }
}
