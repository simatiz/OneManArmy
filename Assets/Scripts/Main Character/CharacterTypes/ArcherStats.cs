using UnityEngine;

public class ArcherStats : CharacterStatsBase
{
    public bool piercingShotActive = false;
    private float piercingShotTimer = 0f;

    protected override void Start()
    {
        base.Start();

        baseDamage = 12f;
        attackRate = 1.2f;
        attackRange = 1f;
        critChance = 0.08f;
        critMultiplier = 2.0f;
        arrowSpeedMultiplier = 1f;
        isRanged = true;
        canAutoHeal = false;

        moveSpeed = 2.5f;
        damageResistance = 1.2f;
}

    protected override void TriggerTransmutationEffect()
    {
        piercingShotActive = true;
        piercingShotTimer = 10f;
        Debug.Log("Archer: Piercing Shot active!");
    }

    protected override void Update()
    {
        base.Update();

        if (piercingShotActive)
        {
            piercingShotTimer -= Time.deltaTime;
            if (piercingShotTimer <= 0f)
                piercingShotActive = false;
        }
    }
}