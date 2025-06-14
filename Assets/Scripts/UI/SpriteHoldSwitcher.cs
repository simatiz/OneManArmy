using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpriteHoldSwitcher : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image targetImage;
    public Sprite pressedSprite;
    private Sprite originalSprite;

    void Start()
    {
        if (targetImage != null)
        {
            originalSprite = targetImage.sprite;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetImage != null && pressedSprite != null)
        {
            targetImage.sprite = pressedSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetImage != null && originalSprite != null)
        {
            targetImage.sprite = originalSprite;
        }
    }
}