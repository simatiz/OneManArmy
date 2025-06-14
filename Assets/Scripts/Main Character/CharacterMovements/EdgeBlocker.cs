using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EdgeBlocker : MonoBehaviour
{
    public enum Level { Sand, Grass, Mountain }
    public Level edgeLevel;

    private Collider2D edgeCollider;

    private void Awake()
    {
        edgeCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryUpdateCollision(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryUpdateCollision(other);
    }

    void TryUpdateCollision(Collider2D collider)
    {
        var player = collider.GetComponent<PlayerMovement>();
        if (player == null) return;

        bool sameLevel = player.GetCurrentLevel() == edgeLevel;

        Physics2D.IgnoreCollision(edgeCollider, collider, !sameLevel);
    }
}