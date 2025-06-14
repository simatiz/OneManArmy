using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public abstract class CharacterStatsBase : MonoBehaviour
{
    [Header("Level")]
    public static int level = 1;

    [Header("Audio SFX")]
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;

    AudioSource sfx;
    public AudioMixerGroup sfxGroup;

    [Header("Health")]
    public static float maxHealth = 100f;
    public static float _currentHealth = 100f;
    public static float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }
    public float damageResistance = 1f;
    public float damageIgnoreChance = 0f;
    public PauseManager UI;
    public Healthbar healthbar;
    private float deathDelay = 2f;
    private Animator animator;
    private static bool healthInitialized = false;

    [Header("Combat")]
    public float baseDamage;
    public float attackRate;
    public float attackRange;
    public float critChance;
    public float critMultiplier;
    public bool isRanged;
    public float arrowSpeedMultiplier = 1f;

    [Header("Movement")]
    public float moveSpeed;
    private float originalSpeed;
    private float bonusSpeedTimer;
    private float bonusSpeedAmount;
    public bool hasAutoSpeedBoost = false;
    public float nextAutoBoostTime = 0f;

    [Header("Status Effects")]
    public bool isDead = false;

    public bool isBurning;
    private float burnTimer;
    private float burnDamage = 3f;

    public bool isPoisoned;
    private float poisonTimer;
    private float poisonDamage = 2f;

    public bool burnOnCrit = false;
    public bool burnOnHit = false;
    public bool bonusVsPoisoned = false;
    public bool hasToxicBoostWhenLow = false;
    public bool hasBloodRefund = false;
    public bool hasBeastSpeedBoost = false;
    public float doubleShotChance = 0f;
    public bool attacksMarkEnemies = false;
    public bool hasSniperBonus = false;
    private bool sniperBonusApplied = false;
    public float sniperBonusRange = 2f;
    public float sniperBonusDamage = 0.15f;

    [Header("Slow Effects")]
    public bool attacksSlow = false;
    public float slowDuration = 0f;
    public float slowChance = 1f;

    [Header("Healing / Buffs")]
    public bool canAutoHeal = false;
    public float autoHealCooldown = 30f;
    public float autoHealPercent = 0.2f;
    private float lastTimeHealed;

    public float onKillHealFactor = 0f;
    public float lastDamageTaken;
    public float lastDamageTakenTime;
    public float lastEnemyHitDamage;

    [Header("Standing")]
    private Vector3 lastPosition;
    public float standingTimer;
    public bool isStandingStill;

    [Header("Toxic Retaliation")]
    public bool retaliatePoisonOnHit = false;

    [Header("Ultimates")]
    public bool comboStrikeEnabled = false;
    public bool hasShieldFortify = false;
    public float fortifyDuration = 10f;
    public float fortifyTimer = 0f;
    public bool hasPiercingShot = false;

    [Header("Transmutation Skull Ability")]
    public bool transmuteReady = true;
    public float transmuteCooldown = 60f;
    private float transmuteTimer = 0f;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        if (!healthInitialized)
        {
            _currentHealth = maxHealth;
            healthInitialized = true;
        }
        lastPosition = transform.position;
        originalSpeed = moveSpeed;

        if (healthbar != null)
        {
            healthbar.SetMaxHealth(maxHealth);
            healthbar.UpdateHealth(_currentHealth);
        }

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 1f;
        sfx.rolloffMode = AudioRolloffMode.Linear;
        sfx.maxDistance = 20f;
        sfx.outputAudioMixerGroup = sfxGroup;
    }

    protected virtual void Update()
    {
        TrackStanding();
        HandleStatusEffects();

        if(hasAutoSpeedBoost && Time.time >= nextAutoBoostTime)
        {
            ActivateBonusSpeed(3f, 5f);
            nextAutoBoostTime = Time.time + 25f;
        }

        if (canAutoHeal && Time.time >= lastTimeHealed + autoHealCooldown)
        {
            Heal(maxHealth * autoHealPercent);
            lastTimeHealed = Time.time;
        }

        if (hasSniperBonus && isStandingStill && standingTimer >= 2f)
        {
            if (!sniperBonusApplied)
            {
                baseDamage *= (1f + sniperBonusDamage);
                attackRange += sniperBonusRange;
                sniperBonusApplied = true;
            }
        }
        else if (sniperBonusApplied)
        {
            baseDamage /= (1f + sniperBonusDamage);
            attackRange -= sniperBonusRange;
            sniperBonusApplied = false;
        }

        if (!transmuteReady)
        {
            transmuteTimer -= Time.deltaTime;
            if (transmuteTimer <= 0f)
            {
                transmuteReady = true;
                Debug.Log("Transmutation Ability Ready Again!");
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && transmuteReady)
        {
            UseTransmutationAbility();
        }
    }

    protected void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfx.pitch = Random.Range(0.9f, 1.1f); // невеличка варіація, щоб не монотонно
        sfx.PlayOneShot(clip, volume);
    }

    public void PlayClip(AudioClip clip, float volume = 1f) => PlaySFX(clip, volume);

    void TrackStanding()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
        {
            standingTimer += Time.deltaTime;
            isStandingStill = true;
        }
        else
        {
            standingTimer = 0f;
            isStandingStill = false;
        }

        lastPosition = transform.position;
    }

    void HandleStatusEffects()
    {
        if (isBurning)
        {
            burnTimer -= Time.deltaTime;
            if (burnTimer <= 0)
            {
                isBurning = false;
            }
            else if (Time.frameCount % 30 == 0)
            {
                TakeDamage(burnDamage, true);
            }
        }

        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            if (poisonTimer <= 0)
            {
                isPoisoned = false;
            }
            else if (Time.frameCount % 30 == 0)
            {
                TakeDamage(poisonDamage, true);
            }
        }
    }

    public virtual void TakeDamage(float amount, bool isStatusEffect = false, EnemyStats attacker = null)
    {
        if (isDead) return;

        if (!isStatusEffect && Random.value < damageIgnoreChance) return;
        if (!isStatusEffect)
            PlaySFX(hurtClip, 0.9f);

        float finalDamage = amount * damageResistance;

        if (hasShieldFortify && Time.time <= fortifyTimer)
        {
            finalDamage *= 0.5f;
        }

        _currentHealth -= finalDamage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        lastDamageTaken = finalDamage;
        lastDamageTakenTime = Time.time;

        healthbar?.UpdateHealth(_currentHealth);

        if (!isStatusEffect && retaliatePoisonOnHit && attacker != null)
        {
            attacker.ApplyPoison(2f);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void DealDamage(float amount, bool killed = false)
    {
        lastEnemyHitDamage = amount;

        if (killed && onKillHealFactor > 0f)
        {
            Heal(amount * onKillHealFactor);
        }

        if (hasBloodRefund && Time.time - lastDamageTakenTime <= 1f)
        {
            Heal(lastDamageTaken * 0.5f);
        }

        if (burnOnCrit && Random.value < critChance)
        {
            ApplyBurn(2f);
        }
    }

    public virtual void Heal(float amount)
    {
        if (isDead || amount <= 0) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        healthbar?.UpdateHealth(_currentHealth);
        lastTimeHealed = Time.time;
    }

    public virtual void Die()
    {
        isDead = true;
        PlaySFX(deathClip, 1f);
        if (animator != null)
        {
            animator.SetBool("Dead", true);
            DisableScripts();
        }

        StartCoroutine(HandleDeathAfterDelay());
    }

    IEnumerator HandleDeathAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);

        UI.ChangeMenu(3);
        UI.Pause();
    }

    void DisableScripts()
    {
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (!(script is Animator  || script == this))
            {
                script.enabled = false;
            }
        }

        // Легкий зсув вгору перед заморозкою
        transform.position += new Vector3(0f, 0.3f, 0f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // повністю зупиняє об’єкт
        }
    }

    public void ApplyBurn(float duration)
    {
        isBurning = true;
        burnTimer = duration;
    }

    public void ApplyPoison(float duration)
    {
        isPoisoned = true;
        poisonTimer = duration;
    }

    public void ActivateBonusSpeed(float amount, float duration)
    {
        moveSpeed = originalSpeed + amount;
        bonusSpeedAmount = amount;
        bonusSpeedTimer = duration;
    }

    public virtual bool HasEffect(ItemType type)
    {
        return InventoryManager.Instance != null && InventoryManager.Instance.HasItem(type);
    }


    public void UseTransmutationAbility()
    {
        if (!transmuteReady || !HasEffect(ItemType.TransmutationSkull)) return;

        transmuteReady = false;
        transmuteTimer = transmuteCooldown;

        Debug.Log($"{gameObject.name} used Transmutation Skull Ability!");

        TriggerTransmutationEffect();
    }

    protected virtual void TriggerTransmutationEffect()
    {
        // Empty — перевизначується у підкласах
    }
}