using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect")]
public class ItemEffect : ScriptableObject
{
    public ItemType type;
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;

    public virtual void ApplyEffect(CharacterStatsBase stats)
    {
        Debug.Log("Default effect for: " + itemName);
    }
}