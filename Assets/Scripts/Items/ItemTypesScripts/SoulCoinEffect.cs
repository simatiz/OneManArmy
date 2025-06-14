using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/SoulCoin")]
public class SoulCoinEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.moveSpeed = 4.5f;
                break;

            case KnightStats:
                stats.damageResistance = 0.3f;
                break;

            case ArcherStats:
                stats.attackRange = 4f;
                break;
        }
    }
}