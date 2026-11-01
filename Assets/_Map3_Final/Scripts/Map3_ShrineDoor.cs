using System.Collections;
using UnityEngine;

namespace Game.Map3
{
    public class Map3_ShrineDoor : Map3_Interactable
    {
        [Header("Teleport Destination")]
        [SerializeField] private Transform targetSpawnPoint;
        [SerializeField] private Vector3 fallbackTargetPosition;
        [SerializeField] private BoxCollider2D targetCameraBounds;
        [SerializeField] private bool isAutoTriggerOnTouch = false;
        [SerializeField] private float fadeDuration = 0.25f;

        private static bool isTeleporting = false;

        public static void ResetTeleportState()
        {
            isTeleporting = false;
        }

        public static bool IsTeleporting => isTeleporting;

        protected override void Awake()
        {
            if (string.IsNullOrEmpty(objectDisplayName))
            {
                objectDisplayName = "Cửa Đền";
            }
            if (string.IsNullOrEmpty(promptText))
            {
                promptText = "[E] Vào Đền";
            }
            base.Awake();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Reset teleport lock if door component gets disabled
            isTeleporting = false;
        }

        private bool IsPlayerCollider(Collider2D col)
        {
            if (col == null) return false;
            if (col.CompareTag("Player")) return true;
            if (col.GetComponent<Map3_PlayerController>() != null) return true;
            if (col.GetComponentInParent<Map3_PlayerController>() != null) return true;
            return false;
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);

            if (IsPlayerCollider(other))
            {
                bool isExitTrigger = gameObject.name.Contains("Exit");
                if (isAutoTriggerOnTouch || isExitTrigger)
                {
                    Interact();
                }
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (IsPlayerCollider(other))
            {
                bool isExitTrigger = gameObject.name.Contains("Exit");
                if ((isAutoTriggerOnTouch || isExitTrigger) && !isTeleporting)
                {
                    Interact();
                }
            }
        }

        public override void Interact()
        {
            if (isTeleporting) return;
            StartCoroutine(TeleportRoutine());
        }

        private IEnumerator TeleportRoutine()
        {
            isTeleporting = true;

            try
            {
                if (Map3_PlayerController.Instance != null)
                {
                    Map3_PlayerController.Instance.SetMovementLocked(true);
                }

                Vector3 dest = Vector3.zero;
                if (targetSpawnPoint != null)
                {
                    dest = targetSpawnPoint.position;
                }
                else if (fallbackTargetPosition != Vector3.zero)
                {
                    dest = fallbackTargetPosition;
                }
                else
                {
                    if (gameObject.name.Contains("Korea") && gameObject.name.Contains("Exit"))
                        dest = new Vector3(9.5f, 1.0f, 0f);
                    else if (gameObject.name.Contains("Viet") && gameObject.name.Contains("Exit"))
                        dest = new Vector3(-6.8f, 1.2f, 0f);
                    else if (gameObject.name.Contains("Korea") && gameObject.name.Contains("Entrance"))
                        dest = new Vector3(50f, 98.5f, 0f);
                    else if (gameObject.name.Contains("Viet") && gameObject.name.Contains("Entrance"))
                        dest = new Vector3(0f, 98.5f, 0f);
                }

                if (targetCameraBounds == null)
                {
                    if (gameObject.name.Contains("Exit"))
                    {
                        var gardenBoundsGo = GameObject.Find("Garden_Camera_Bounds");
                        if (gardenBoundsGo != null) targetCameraBounds = gardenBoundsGo.GetComponent<BoxCollider2D>();
                    }
                    else if (gameObject.name.Contains("Korea"))
                    {
                        var koreaBoundsGo = GameObject.Find("KoreaShrine_Camera_Bounds");
                        if (koreaBoundsGo != null) targetCameraBounds = koreaBoundsGo.GetComponent<BoxCollider2D>();
                    }
                    else if (gameObject.name.Contains("Viet"))
                    {
                        var vietBoundsGo = GameObject.Find("VietShrine_Camera_Bounds");
                        if (vietBoundsGo != null) targetCameraBounds = vietBoundsGo.GetComponent<BoxCollider2D>();
                    }
                }

                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.FadeOutIn(fadeDuration, () =>
                    {
                        if (Map3_PlayerController.Instance != null)
                        {
                            Map3_PlayerController.Instance.TeleportTo(dest);
                        }

                        if (Map3_CameraController.Instance != null && targetCameraBounds != null)
                        {
                            Map3_CameraController.Instance.SetBounds(targetCameraBounds, snapImmediate: true);
                        }
                    });

                    // Wait for the complete Fade Out (fadeDuration) + hold (0.05s) + Fade In (fadeDuration)
                    yield return new WaitForSeconds(fadeDuration * 2f + 0.1f);
                }
                else
                {
                    if (Map3_PlayerController.Instance != null)
                    {
                        Map3_PlayerController.Instance.TeleportTo(dest);
                    }

                    if (Map3_CameraController.Instance != null && targetCameraBounds != null)
                    {
                        Map3_CameraController.Instance.SetBounds(targetCameraBounds, snapImmediate: true);
                    }
                }

                if (Map3_PlayerController.Instance != null)
                {
                    Map3_PlayerController.Instance.SetMovementLocked(false);
                }

                // Cooldown to prevent immediate bounce back
                yield return new WaitForSeconds(0.4f);
            }
            finally
            {
                isTeleporting = false;
            }
        }
    }
}
