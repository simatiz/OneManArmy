using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/FlameCrystal")]
public class FlameCrystalEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.baseDamage = 11f;
                stats.burnOnCrit = true;
                break;

            case KnightStats:
                stats.baseDamage = 30f;
                // -25% шкоди від підпалених ворогів (обробляється в EnemyStats)
                break;

            case ArcherStats:
                stats.burnOnHit = true;
                break;
        }
    }
}