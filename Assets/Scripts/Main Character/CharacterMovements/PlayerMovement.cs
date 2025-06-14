using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float ladderSpeed = 2f;

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStatsBase stats;
    private SpriteRenderer spR;
    private Vector2 movement;
    private bool isFacingRight = true;
    public bool canMove = true;

    public Tilemap sandTilemap, grassTilemap, mountainTilemap;
    public Tilemap sandBridgeTilemap, grassBridgeTilemap, mountainBridgeTilemap;
    public Tilemap wallTilemap, higherWallTilemap;
    public Tilemap grassLadderTilemap, mountainLadderTilemap;
    public Transform sandColliderParent, grassColliderParent, mountainColliderParent;
    public Transform sandBridgeColliderParent, grassBridgeColliderParent, mountainBridgeColliderParent;

    private TilemapCollider2D sandCollider, grassCollider, mountainCollider;
    private TilemapCollider2D sandBridgeCollider, grassBridgeCollider, mountainBridgeCollider;

    private enum HeightLevel { Sand, Grass, Mountain }
    private HeightLevel currentLevel = HeightLevel.Sand;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStatsBase>();
        spR = GetComponent<SpriteRenderer>();

        sandCollider = sandTilemap.GetComponent<TilemapCollider2D>();
        grassCollider = grassTilemap.GetComponent<TilemapCollider2D>();
        mountainCollider = mountainTilemap.GetComponent<TilemapCollider2D>();

        sandBridgeCollider = sandBridgeTilemap.GetComponent<TilemapCollider2D>();
        grassBridgeCollider = grassBridgeTilemap.GetComponent<TilemapCollider2D>();
        mountainBridgeCollider = mountainBridgeTilemap.GetComponent<TilemapCollider2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.magnitude > 1)
            movement.Normalize();

        if (movement.x > 0 && !isFacingRight)
            Flip();
        else if (movement.x < 0 && isFacingRight)
            Flip();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            return;
        }

        rb.velocity = new Vector2(movement.x * stats.moveSpeed, movement.y * stats.moveSpeed);
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        var scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("SandBridge"))
        {
            EnableColliders(sandColliderParent, false);
            EnableColliders(sandBridgeColliderParent, true);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("GrassBridge"))
        {
            Debug.Log("Enter Grass Bridge");
            EnableColliders(grassColliderParent, false);
            EnableColliders(grassBridgeColliderParent, true);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("MountainBridge"))
        {
            EnableColliders(mountainColliderParent, false);
            EnableColliders(mountainBridgeColliderParent, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("SandBridge"))
        {
            EnableColliders(sandColliderParent, true);
            EnableColliders(sandBridgeColliderParent, false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("GrassBridge"))
        {
            EnableColliders(grassColliderParent, true);
            EnableColliders(grassBridgeColliderParent, false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("MountainBridge"))
        {
            EnableColliders(mountainColliderParent, true);
            EnableColliders(mountainBridgeColliderParent, false);
        }
    }

    private void SetHeightLevel(HeightLevel level)
    {
        if (currentLevel != level)
        {
            currentLevel = level;
            UpdateColliders();
            Debug.Log("LEVEL → " + currentLevel);
        }
    }

    public EdgeBlocker.Level GetCurrentLevel()
    {
        Debug.Log(currentLevel);
        switch (currentLevel)
        {
            case HeightLevel.Sand: return EdgeBlocker.Level.Sand;
            case HeightLevel.Grass: return EdgeBlocker.Level.Grass;
            case HeightLevel.Mountain: return EdgeBlocker.Level.Mountain;
            default: return EdgeBlocker.Level.Sand;
        }
    }

    void UpdateColliders()
    {
        EnableColliders(sandColliderParent, currentLevel == HeightLevel.Sand);
        EnableColliders(sandBridgeColliderParent, false);
        EnableColliders(grassColliderParent, currentLevel == HeightLevel.Grass);
        EnableColliders(grassBridgeColliderParent, false);
        EnableColliders(mountainColliderParent, currentLevel == HeightLevel.Mountain);
        EnableColliders(mountainBridgeColliderParent, false);

        UpdateWallVisibility();
        UpdateLevels();
        UpdatePlayerLayer();
    }

    void UpdateWallVisibility()
    {
        if (wallTilemap != null)
        {
            bool enableMountainWalls = currentLevel == HeightLevel.Grass;
            wallTilemap.GetComponent<TilemapCollider2D>().enabled = enableMountainWalls;
        }

        if (higherWallTilemap != null)
        {
            bool enableGrassWalls = currentLevel == HeightLevel.Sand;
            higherWallTilemap.GetComponent<TilemapCollider2D>().enabled = enableGrassWalls;
        }
    }

    void UpdateLevels()
    {
        if (currentLevel == HeightLevel.Sand)
        {
            grassCollider.enabled = true;
            mountainCollider.enabled = true;
            grassBridgeCollider.isTrigger = false;
            grassBridgeCollider.enabled = false;
            mountainBridgeCollider.isTrigger = false;
            mountainBridgeCollider.enabled = false;
        }

        else if(currentLevel == HeightLevel.Grass)
        {
            grassCollider.enabled = false;
            mountainCollider.enabled = true;
            grassBridgeCollider.isTrigger = true;
            grassBridgeCollider.enabled = true;
            mountainBridgeCollider.isTrigger = false;
            mountainBridgeCollider.enabled = false;
        }
        else if (currentLevel == HeightLevel.Mountain)
        {
            grassCollider.enabled = false;
            mountainCollider.enabled = false;
            grassBridgeCollider.isTrigger = false;
            grassBridgeCollider.enabled = false;
            mountainBridgeCollider.isTrigger = true;
            mountainBridgeCollider.enabled = true;
        }
    }

    void EnableColliders(Transform parent, bool enable)
    {
        foreach (Transform child in parent)
        {
            Collider2D col = child.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = enable;
            }
        }
    }

    void UpdatePlayerLayer()
    {
        if (currentLevel == HeightLevel.Sand) spR.sortingOrder = 6;
        else if (currentLevel == HeightLevel.Grass) spR.sortingOrder = 8;
        else spR.sortingOrder = 10;
    }

    public void SetHeightLevelToGrass()
    {
        SetHeightLevel(HeightLevel.Grass);
    }

    public void SetHeightLevelToMountain()
    {
        SetHeightLevel(HeightLevel.Mountain);
    }

    public void SetHeightLevelToSand()
    {
        SetHeightLevel(HeightLevel.Sand);
    }

    public void SetNewHeightLevel(EdgeBlocker.Level level)
    {
        HeightLevel newLevel;
        switch (level)
        {
            case EdgeBlocker.Level.Sand: newLevel = HeightLevel.Sand;
                break;
            case EdgeBlocker.Level.Grass: newLevel = HeightLevel.Grass;
                break;
            case EdgeBlocker.Level.Mountain: newLevel = HeightLevel.Mountain;
                break;
            default: newLevel = HeightLevel.Sand;
                break;
        }
        SetHeightLevel(newLevel);
    }
}