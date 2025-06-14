public class SlaveStats : CharacterStatsBase
{
    protected override void Start()
    {
        base.Start();

        baseDamage = 8f;
        attackRate = 3f;
        attackRange = 2.5f;
        critChance = 0.10f;
        critMultiplier = 2.5f;
        isRanged = false;

        moveSpeed = 3.5f;
        damageResistance = 1.0f;

        onKillHealFactor = 0f;
    }

    protected override void TriggerTransmutationEffect()
    {
        comboStrikeEnabled = true;
    }
}