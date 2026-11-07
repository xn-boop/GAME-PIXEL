using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Map0
{
    public class Map0_VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private Map0_PlayerController playerController;
        [SerializeField] private float handleRange = 60f;

        private RectTransform baseRect;
        private Vector2 inputVector = Vector2.zero;

        public Vector2 InputVector => inputVector;

        private void Awake()
        {
            baseRect = GetComponent<RectTransform>();
            if (playerController == null)
            {
                playerController = Map0_PlayerController.Instance;
            }
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = Map0_PlayerController.Instance;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (baseRect == null) return;

            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, eventData.pressEventCamera, out pos))
            {
                pos.x /= baseRect.sizeDelta.x;
                pos.y /= baseRect.sizeDelta.y;

                inputVector = new Vector2(pos.x * 2f, pos.y * 2f);
                inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

                if (joystickHandle != null)
                {
                    joystickHandle.anchoredPosition = new Vector2(
                        inputVector.x * (baseRect.sizeDelta.x / 2f) * (handleRange / 100f),
                        inputVector.y * (baseRect.sizeDelta.y / 2f) * (handleRange / 100f)
                    );
                }

                if (playerController != null)
                {
                    playerController.SetJoystickInput(inputVector);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }

            if (playerController != null)
            {
                playerController.SetJoystickInput(Vector2.zero);
            }
        }

        private void OnDisable()
        {
            inputVector = Vector2.zero;
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
