using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/LifeFlask")]
public class LifeFlaskEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.hasBloodRefund = true;
                break;

            case KnightStats:
                stats.damageIgnoreChance = 0.2f;
                break;

            case ArcherStats:
                stats.canAutoHeal = true;
                stats.autoHealCooldown = 5f;
                stats.autoHealPercent = 0.05f;
                break;
        }
    }
}