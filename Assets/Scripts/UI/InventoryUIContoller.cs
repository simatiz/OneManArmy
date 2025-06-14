using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    public List<InventorySlotUI> uiSlots;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }

        InventoryManager.Instance.AssignUISlots(uiSlots);
        InventoryManager.Instance.AssignTexts(itemNameText, descriptionText);
        InventoryManager.Instance.ApplyToAllCharacters();
    }
}