using UnityEngine;

public class KnightStats : CharacterStatsBase
{
    protected override void Start()
    {
        base.Start();

        baseDamage = 20f;
        attackRate = 2f;
        attackRange = 2.5f;
        critChance = 0.05f;
        critMultiplier = 1.5f;
        isRanged = false;

        moveSpeed = 2.0f;
        damageResistance = 0.5f;
    }

    protected override void TriggerTransmutationEffect()
    {
        fortifyTimer = Time.time + fortifyDuration;
    }
}