using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image icon;
    private ItemEffect currentItem;

    void Awake()
    {
        icon = GetComponent<Image>();
        if (icon == null)
            Debug.LogWarning("InventorySlotUI: Image компонент не знайдено!");
    }

    public void SetItem(ItemEffect item)
    {
        currentItem = item;
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.color = new Color(1f, 1f, 1f, 1f);
            icon.enabled = true;
        }
    }

    public void Clear()
    {
        currentItem = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.color = new Color(0f, 0f, 0f, 0f);
            icon.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
            InventoryManager.Instance.ShowItemNameAndDescription(currentItem.itemName, currentItem.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.HideItemName();
    }
}