using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/TransmutationSkull")]
public class TransmutationSkullEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.comboStrikeEnabled = true;
                break;

            case KnightStats:
                stats.hasShieldFortify = true;
                stats.fortifyTimer = Time.time + stats.fortifyDuration;
                break;

            case ArcherStats:
                stats.hasPiercingShot = true;
                break;
        }
    }
}