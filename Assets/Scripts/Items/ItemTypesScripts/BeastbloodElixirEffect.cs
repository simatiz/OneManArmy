using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/BeastbloodElixir")]
public class BeastbloodElixir : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.hasBeastSpeedBoost = true;
                break;

            case KnightStats:
                stats.critMultiplier = 2f;
                break;

            case ArcherStats:
                stats.doubleShotChance = 0.25f;
                break;
        }
    }
}