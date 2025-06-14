using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float maxTravelDistance = 1f;

    public CharacterStatsBase owner;
    public Vector2 direction = Vector2.right;
    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;
        GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;

        if (owner.hasPiercingShot)
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }

    private void Update()
    {
        // Збільшення швидкості (якщо є баф)
        speed *= owner.arrowSpeedMultiplier;

        // Знищення при досягненні дистанції
        if (Vector2.Distance(startPosition, transform.position) >= maxTravelDistance)
        {
            Destroy(gameObject);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.1f, LayerMask.GetMask("ProjectileBlocker"));
        if (hit.collider != null && hit.collider.CompareTag("ProjectileBlock"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("ProjectileBlocker"))
        {
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("ProjectileBlock"))
        {
            Destroy(gameObject);
            return;
        }

        EnemyStats target = other.GetComponent<EnemyStats>();
        if (target != null)
        {
            float finalDamage = damage;

            if (target.isPoisoned && owner.bonusVsPoisoned)
                finalDamage *= 1.3f;

            if (owner.attacksSlow)
            {
                target.ApplySlow(owner.slowDuration);
            }

            if (owner.hasPiercingShot)
            {
                finalDamage *= 1.5f;
                target.TakeDamage(finalDamage);
                owner.DealDamage(finalDamage, target.IsDead());
                return; // не знищуємо стрілу — вона проходить крізь
            }

            target.TakeDamage(finalDamage);

            if (owner.burnOnHit)
                target.ApplyBurn(1f);

            owner.DealDamage(finalDamage, target.IsDead());
            Destroy(gameObject);
        }
    }
}