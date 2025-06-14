using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/MapFragment")]
public class MapFragmentEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        Debug.Log("Map Fragment collected.");
    }
}