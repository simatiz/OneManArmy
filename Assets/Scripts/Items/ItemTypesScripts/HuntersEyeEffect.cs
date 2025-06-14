using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/HuntersEye")]
public class HuntersEyeEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.onKillHealFactor = 0.75f;
                break;

            case KnightStats:
                stats.attacksMarkEnemies = true;
                break;

            case ArcherStats:
                stats.hasSniperBonus = true;
                break;
        }
    }
}