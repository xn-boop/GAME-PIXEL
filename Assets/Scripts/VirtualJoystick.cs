
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("Joystick References")]
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Joystick Settings")]
    [SerializeField] private float handleRange = 50f;

    private RectTransform joystickBase;

    private void Awake()
    {
        joystickBase = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBase == null || joystickHandle == null ||
            playerMovement == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            return;
        }

        Vector2 input = Vector2.ClampMagnitude(
            localPoint / handleRange, 1f);

        joystickHandle.anchoredPosition = input * handleRange;
        playerMovement.SetMoveInput(input);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.StopMovement();
        }
    }
}
