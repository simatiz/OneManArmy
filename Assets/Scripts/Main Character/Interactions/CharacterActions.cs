using System.Collections;
using UnityEngine;

public class CharacterActions : MonoBehaviour
{
    private CharacterStatsBase stats;
    private Animator anim;
    private PlayerMovement movementScript;
    private float attackCooldown = 0f;
    private Vector2 lastMoveDirection = Vector2.right;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask enemyLayers;

    private int comboStep = 0;
    private float comboTimer = 0f;
    public float comboResetTime = 2f;

    void Start()
    {
        stats = GetComponent<CharacterStatsBase>();
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                comboStep = 0;
                
                anim.SetInteger("ComboStep", 0);
            }
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            lastMoveDirection = input.normalized;
        }

        if (Input.GetKeyDown(KeyCode.F) && stats.isRanged)
        {
            TryShoot();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            TryAttack();
        }
    }

    public void TryAttack()
    {
        if (attackCooldown > 0) return;

        attackCooldown = 1f / stats.attackRate;

        // Логіка комбо
        comboStep = (comboStep % 2) + 1; // циклічно 1 → 2 → 1 → 2
        comboTimer = comboResetTime;

        anim.SetFloat("AttackY", Mathf.Abs(lastMoveDirection.y));
        anim.SetFloat("AttackY2", lastMoveDirection.y);
        anim.SetBool("Attack", true);

        anim.SetInteger("ComboStep", comboStep);
        movementScript.canMove = false;
        StartCoroutine(StopAttackAnimation(anim, 0.3f));
        stats.PlayClip(stats.attackClip, 1f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats target = enemy.GetComponent<EnemyStats>();
            if (target != null)
            {
                float damage = stats.baseDamage;

                bool isCrit = Random.value < stats.critChance;
                if (isCrit) damage *= stats.critMultiplier;

                if (stats.hasToxicBoostWhenLow && CharacterStatsBase._currentHealth < CharacterStatsBase.maxHealth * 0.5f)
                {
                    damage *= 1.2f;
                }

                if (stats.attacksMarkEnemies)
                {
                    target.isMarked = true;
                }

                if (stats.attacksSlow && Random.value < stats.slowChance)
                {
                    target.ApplySlow(stats.slowDuration);
                }

                target.TakeDamage(damage);

                if (stats.comboStrikeEnabled)
                {
                    StartCoroutine(ComboStrike(target));
                }

                stats.DealDamage(damage, target.IsDead());
            }
        }
    }

    private System.Collections.IEnumerator StopAttackAnimation(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetBool("Attack", false);
        movementScript.canMove = true;
    }

    public void TryShoot()
    {
        if (attackCooldown > 0) return;

        attackCooldown = 1f / stats.attackRate;
        Vector2 shootDir = lastMoveDirection;
        if (shootDir == Vector2.zero)
            shootDir = Vector2.right;
        if (stats.TryGetComponent<Animator>(out var anim))
        {
            anim.SetBool("Shoot", true);
            movementScript.canMove = false;
            anim.SetFloat("DirectionX", Mathf.Abs(lastMoveDirection.x));
            anim.SetFloat("DirectionY", lastMoveDirection.y);
        }
        stats.PlayClip(stats.attackClip, 1f);
        StartCoroutine(DelayedShoot(shootDir));
    }

    private IEnumerator DelayedShoot(Vector2 shootDir)
    {
        yield return new WaitForSeconds(0.6f);
        anim.SetBool("Shoot", false);

        GameObject arrow = Instantiate(Resources.Load("Arrow")) as GameObject;
        arrow.transform.position = (Vector2)attackPoint.position + shootDir.normalized * 0.5f;

        // Встановити напрямок стріли (тільки візуально)
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Запуск руху
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.velocity = shootDir.normalized * arrowScript.speed;

        // Додаткова інформація
        arrowScript.owner = stats;
        arrowScript.damage = stats.baseDamage;
        arrowScript.direction = shootDir;

        // Подвійний постріл
        if (Random.value < stats.doubleShotChance)
        {
            GameObject secondArrow = Instantiate(arrow, arrow.transform.position, arrow.transform.rotation);
            var secondScript = secondArrow.GetComponent<Arrow>();
            secondScript.direction = shootDir;
        }
        movementScript.canMove = true;
    }

    private System.Collections.IEnumerator ComboStrike(EnemyStats target)
    {
        for (int i = 0; i < 3; i++)
        {
            float damage = stats.baseDamage;
            if (Random.value < stats.critChance + 0.25f) // +25% шанс на крит
                damage *= stats.critMultiplier;

            target.TakeDamage(damage);
            stats.DealDamage(damage, target.IsDead());

            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}