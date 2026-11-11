using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Map3
{
    public class Map3_VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick References")]
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private Map3_PlayerController playerController;

        [Header("Joystick Settings")]
        [SerializeField] private float handleRange = 60f;

        private RectTransform joystickBase;

        private void Awake()
        {
            joystickBase = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = Map3_PlayerController.Instance;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (joystickBase == null || joystickHandle == null) return;

            if (playerController == null)
            {
                playerController = Map3_PlayerController.Instance;
                if (playerController == null) return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBase,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                Vector2 input = Vector2.ClampMagnitude(localPoint / handleRange, 1f);
                joystickHandle.anchoredPosition = input * handleRange;
                playerController.SetJoystickInput(input);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }

            if (playerController != null)
            {
                playerController.SetJoystickInput(Vector2.zero);
            }
        }
    }
}
