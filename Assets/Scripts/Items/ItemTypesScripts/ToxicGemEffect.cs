using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/ToxicEmerald")]
public class ToxicEmeraldEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.retaliatePoisonOnHit = true;
                break;

            case KnightStats:
                stats.hasToxicBoostWhenLow = true;
                break;

            case ArcherStats:
                stats.bonusVsPoisoned = true;
                break;
        }
    }
}