using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextPressOffset : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI targetText;
    public Vector2 offset = new Vector2(0, -4f);

    private Vector3 originalPosition;

    void Start()
    {
        if (targetText != null)
        {
            originalPosition = targetText.rectTransform.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetText != null)
        {
            targetText.rectTransform.anchoredPosition = originalPosition + (Vector3)offset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetText != null)
        {
            targetText.rectTransform.anchoredPosition = originalPosition;
        }
    }
}