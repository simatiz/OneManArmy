using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<ItemEffect> items = new List<ItemEffect>();
    public List<InventorySlotUI> slots = new List<InventorySlotUI>();
    public int maxItems = 9;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;
    public int mapFragmentSlotIndex = 8; // 9-та ячейка (індекс 8)
    public ItemEffect fullMap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        itemNameText.text = "Empty";
        descriptionText.text = "What to describe?";
    }

    public bool AddItem(ItemEffect item)
    {
        if (item.type == ItemType.MapFragment)
        {
            int currentFragments = items.Count(i => i.type == ItemType.MapFragment);
            if (currentFragments >= 4) return false; // вже максимум

            items.Add(item);
            UpdateInventoryUI(items);

            if (currentFragments + 1 == 4)
            {
                // Замінюємо фрагменти на карту
                items.RemoveAll(i => i.type == ItemType.MapFragment);

                items.Add(fullMap);
                UpdateInventoryUI(items);
                if (fullMap == null)
                {
                    Debug.LogError("FullMap is null!");
                }
                else if (!(fullMap is FullMapEffect))
                {
                    Debug.LogError($"FullMap is not of type FullMapEffect! It's actually {fullMap.GetType()}");
                }
                if (fullMap is FullMapEffect fullMapEffect)
                {
                    fullMapEffect.ApplyEffect(FindObjectOfType<CharacterStatsBase>());
                    items.RemoveAll(i => i.type == ItemType.FullMap);
                }
                else
                {
                    Debug.LogError("FullMap is not of type FullMapEffect!");
                }
            }

            return true;
        }

        if (items.Count(i => i.type != ItemType.MapFragment) >= maxItems - 1) return false;

        items.Add(item);
        ApplyToAllCharacters();
        UpdateInventoryUI(items);
        return true;
    }

    public bool HasItem(ItemType type) => items.Any(i => i.type == type);
    public int CountOf(ItemType type) => items.Count(i => i.type == type);

    public void ApplyToAllCharacters()
    {
        foreach (var stats in FindObjectsOfType<CharacterStatsBase>())
        {
            foreach (var item in items)
            {
                item.ApplyEffect(stats: stats);
            }
        }
    }

    public void ReapplyTo(CharacterStatsBase stats)
    {
        foreach (var item in items)
        {
            item.ApplyEffect(stats: stats);
        }
    }

    public void UpdateInventoryUI(List<ItemEffect> items)
    {
        foreach (var slot in slots)
            slot.Clear();

        var mapFragments = items.Where(i => i.type == ItemType.MapFragment || i.type == ItemType.FullMap).ToList();
        var otherItems = items.Where(i => i.type != ItemType.MapFragment && i.type != ItemType.FullMap).ToList();

        if (mapFragments.Count > 0)
            slots[mapFragmentSlotIndex].SetItem(mapFragments[0]); // показуємо лише один

        int index = 0;
        for (int i = 0; i < otherItems.Count && index < slots.Count; i++)
        {
            if (index == mapFragmentSlotIndex) index++; // пропустити слот для фрагментів
            if (index >= slots.Count) break;

            slots[index].SetItem(otherItems[i]);
            index++;
        }
    }

    public void ShowItemNameAndDescription(string name, string description)
    {
        itemNameText.text = name;
        descriptionText.text = description;
    }

    public void HideItemName()
    {
        itemNameText.text = "Empty";
        descriptionText.text = "What to describe?";
    }

    public void AssignUISlots(List<InventorySlotUI> newSlots)
    {
        slots = newSlots;
        UpdateInventoryUI(items);
    }

    public void AssignTexts(TextMeshProUGUI itemName, TextMeshProUGUI description)
    {
        itemNameText = itemName;
        descriptionText = description;
    }
}