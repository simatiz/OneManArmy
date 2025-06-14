using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class EnemyStats : MonoBehaviour
{
    [Header("Scene change on death")]
    public bool loadSceneOnDeath = false;
    public string sceneToLoad = "Win";

    [Header("Sound FX")]
    public AudioClip hitClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;
    AudioSource sfx;
    public AudioMixerGroup sfxGroup;

    public float maxHealth = 100f;
    public float _currentHealth = 100f;
    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }
    public float baseDamage = 10f;

    public bool isSlowed = false;
    private float slowTimer = 0f;
    public float speed = 2f;
    private float originalSpeed;

    public bool isDead = false;
    public bool isPoisoned = false;
    public bool isBurning = false;
    public bool isMarked = false;

    private float poisonTimer = 0f;
    private float burnTimer = 0f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;

    public Healthbar healthbar;

    private void Start()
    {
        _currentHealth = maxHealth;
        originalSpeed = speed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        healthbar.SetMaxHealth(maxHealth);

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 1f;
        sfx.rolloffMode = AudioRolloffMode.Linear;
        sfx.maxDistance = 20f;
        sfx.outputAudioMixerGroup = sfxGroup;
    }

    private void Update()
    {
        HandlePoison();
        HandleBurn();
        HandleSlow();
    }

    protected void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfx.pitch = Random.Range(0.9f, 1.1f); // невеличка варіація, щоб не монотонно
        sfx.PlayOneShot(clip, volume);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float finalDamage = amount;

        if (isMarked)
            finalDamage *= 1.4f;

        _currentHealth -= finalDamage;
        healthbar.UpdateHealth(_currentHealth);
        PlaySFX(hurtClip, 0.9f);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyPoison(float duration)
    {
        isPoisoned = true;
        poisonTimer = duration;
        FlashColor(Color.green);
    }

    public void ApplyBurn(float duration)
    {
        isBurning = true;
        burnTimer = duration;
        FlashColor(Color.red);
    }

    public void ApplySlow(float duration)
    {
        isSlowed = true;
        slowTimer = duration;
        speed = originalSpeed * 0.5f;
    }

    private void HandlePoison()
    {
        if (!isPoisoned) return;

        poisonTimer -= Time.deltaTime;
        if (poisonTimer <= 0)
        {
            isPoisoned = false;
            ResetColor();
        }
        else if (Time.frameCount % 30 == 0)
        {
            TakeDamage(2f);
        }
    }

    private void HandleSlow()
    {
        if (!isSlowed) return;

        slowTimer -= Time.deltaTime;
        if (slowTimer <= 0)
        {
            isSlowed = false;
            speed = originalSpeed;
        }
    }

    private void HandleBurn()
    {
        if (!isBurning) return;

        burnTimer -= Time.deltaTime;
        if (burnTimer <= 0)
        {
            isBurning = false;
            ResetColor();
        }
        else if (Time.frameCount % 30 == 0)
        {
            TakeDamage(3f);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void Die()
    {
        isDead = true;
        PlaySFX(deathClip, 0.9f);
        if (healthbar != null)
        {
            Destroy(healthbar.gameObject);
        }
        if (animator != null)
        {
            animator.SetBool("Dead", true);
            DisableScripts();
        }

        StartCoroutine(HandleDeathAfterDelay(0.5f));
    }

    private void FlashColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public void AttackTarget(CharacterStatsBase target)
    {
        float actualDamage = baseDamage;

        if (isBurning && target is KnightStats && target.HasEffect(ItemType.FlameCrystal))
        {
            actualDamage *= 0.75f; // -25% шкоди
        }
        PlaySFX(hitClip, 0.9f);
        target.TakeDamage(actualDamage);
    }

    void DisableScripts()
    {
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (!(script is Animator || script == this))
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

    IEnumerator HandleDeathAfterDelay(float deathDelay)
    {
        yield return new WaitForSeconds(deathDelay);

        if (loadSceneOnDeath && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }

        gameObject.SetActive(false);
    }
}