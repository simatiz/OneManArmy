using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/SwiftbrewTonic")]
public class SwiftbrewTonicEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        switch (stats)
        {
            case SlaveStats:
                stats.hasAutoSpeedBoost = true;
                stats.nextAutoBoostTime = Time.time + 1f; // перша активація через 1с
                break;

            case KnightStats:
                stats.moveSpeed = 2.5f;
                break;

            case ArcherStats archer:
                archer.arrowSpeedMultiplier = 1.5f;
                break;
        }
    }
}