using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/FrostGem")]
public class FrostGemEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.attacksSlow = true;
                stats.slowDuration = 1.0f;
                stats.slowChance = 0.25f;
                break;

            case KnightStats:
                stats.attacksSlow = true;
                stats.slowDuration = 1.5f;
                break;

            case ArcherStats:
                stats.attacksSlow = true;
                stats.slowDuration = 2.0f;
                break;
        }
    }
}