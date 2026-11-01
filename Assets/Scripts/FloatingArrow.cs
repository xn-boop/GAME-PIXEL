using UnityEngine;

public class FloatingArrow : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float height = 0.12f;
    private Vector3 initialPos;

    void Start()
    {
        initialPos = transform.localPosition;
    }

    void Update()
    {
        float newY = initialPos.y + Mathf.Sin(Time.time * speed) * height;
        transform.localPosition = new Vector3(initialPos.x, newY, initialPos.z);
    }
}