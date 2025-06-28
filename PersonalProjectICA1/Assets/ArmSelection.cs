using UnityEngine;
using UnityEngine.EventSystems;

public class ArmSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float hoverOffset = 20f;
    [SerializeField] float moveSpeed = 10f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private bool isHovering;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        targetPosition = originalPosition;
    }

    private void OnEnable()
    {
        rectTransform.anchoredPosition = originalPosition;
        targetPosition = originalPosition;
    }
    private void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            Time.unscaledDeltaTime * moveSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetPosition = originalPosition + new Vector2(0, hoverOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetPosition = originalPosition;
    }
}
