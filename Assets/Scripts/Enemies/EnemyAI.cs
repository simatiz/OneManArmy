using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    [Header("Behaviour")]
    public float detectionRadius = 5f;
    public float attackRadius = 2.5f;
    public float attackCooldown = 1f;
    public float retreatTime = 0.5f;
    public float moveSpeed = 2f;
    public float pathRefresh = 1.5f;

    [Header("Level Binding")]
    public EdgeBlocker.Level allowedLevel;
    public IslandGenerator islandGenerator;

    [Header("Visual")]
    public GameObject healthbar;

    Transform player;
    EnemyStats stats;
    Animator anim;
    Renderer rend;
    SimpleGridPathfinder path;

    List<Vector3> curPath;
    int pathIdx;
    float pathTimer;
    bool isRetreat;
    float retreatTimer;
    Vector3 spawnPos;
    static readonly Vector3 VISUAL_OFFSET = Vector3.down * 0.9f;
    Vector3 logicalSpawn;

    void Start()
    {
        player = CharacterSwitcher.currentPlayer?.transform;
        stats = GetComponent<EnemyStats>();
        anim = GetComponent<Animator>();
        rend = GetComponent<Renderer>();
        spawnPos = transform.position;
        logicalSpawn = spawnPos - VISUAL_OFFSET;

        path = gameObject.AddComponent<SimpleGridPathfinder>();
        path.Initialize(
            islandGenerator.sandTilemap,
            islandGenerator.grassTilemap,
            islandGenerator.mountainTilemap,
            new[] {
                islandGenerator.wallTilemap,
                islandGenerator.higherWallTilemap,
                islandGenerator.grassLadderTilemap,
                islandGenerator.mountainLadderTilemap
            },
            allowedLevel);
    }

    void Update()
    {
        if (!rend.isVisible) return;
        if (stats.isDead) return;
        if (CharacterSwitcher.currentPlayer != null)
            player = CharacterSwitcher.currentPlayer.transform;
        if (player == null) return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        bool sameLvl = pm != null && pm.GetCurrentLevel() == allowedLevel;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (isRetreat)
        {
            retreatTimer -= Time.deltaTime;
            if (retreatTimer <= 0f) isRetreat = false;
            FollowPath(moveSpeed);
            return;
        }

        if (sameLvl && distToPlayer <= attackRadius)
        {
            Face(player.position - transform.position);
            if ((attackCooldown -= Time.deltaTime) <= 0f)
            {
                if (player.TryGetComponent(out CharacterStatsBase target))
                    StartCoroutine(Attack(target));
                attackCooldown = 1f;
            }
            return;
        }

        pathTimer -= Time.deltaTime;

        if (sameLvl && distToPlayer <= detectionRadius)
        {
            if (NeedsPath()) Repath(player.position);
        }
        else
        {
            if (NeedsPath()) Repath(GetRandomPatrolPoint());
        }

        FollowPath(moveSpeed);
    }

    bool NeedsPath() => curPath == null || pathIdx >= curPath.Count || pathTimer <= 0f;

    void Repath(Vector3 logicalTarget)
    {
        Vector3 logicalSelf = transform.position - VISUAL_OFFSET;

        curPath = path.FindPath(logicalSelf, logicalTarget);
        pathIdx = 0;
        pathTimer = pathRefresh;
    }

    void FollowPath(float speed)
    {
        if (curPath == null || pathIdx >= curPath.Count)
        { anim.SetFloat("Speed", 0); return; }

        Vector3 dst = curPath[pathIdx] + VISUAL_OFFSET;
        Vector3 dir = dst - transform.position;
        float step = speed * Time.deltaTime;

        if (dir.magnitude <= step) { transform.position = dst; pathIdx++; }
        else { transform.position += dir.normalized * step; }

        Face(dir);
        anim.SetFloat("Speed", speed);
    }

    void Face(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) < 0.01f) return;
        bool right = dir.x > 0;
        transform.localScale = new Vector3(right ? 1 : -1, 1, 1);
        healthbar.transform.localScale = new Vector3(right ? 0.05f : -0.05f, 0.05f, 0.05f);
    }

    IEnumerator Attack(CharacterStatsBase target)
    {
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(0.2f);
        stats.AttackTarget(target);
        yield return new WaitForSeconds(0.3f);
        anim.SetBool("Attack", false);

        isRetreat = true;
        retreatTimer = retreatTime;
        Repath(logicalSpawn);
    }

    Vector3 GetRandomPatrolPoint()
    {
        Tilemap map = GetLevelMap();
        map.CompressBounds();
        List<Vector3Int> cells = new();

        foreach (var c in map.cellBounds.allPositionsWithin)
            if (IsCellWalkable(c)) cells.Add(c);

        if (cells.Count == 0) return transform.position;
        return map.GetCellCenterWorld(cells[Random.Range(0, cells.Count)]);
    }

    bool IsCellWalkable(Vector3Int c)
    {
        if (!path.IsTopmostTile(c)) return false;
        if (islandGenerator.wallTilemap.HasTile(c) ||
            islandGenerator.higherWallTilemap.HasTile(c) ||
            islandGenerator.grassLadderTilemap.HasTile(c) ||
            islandGenerator.mountainLadderTilemap.HasTile(c))
            return false;
        return true;
    }

    Tilemap GetLevelMap() => allowedLevel switch
    {
        EdgeBlocker.Level.Sand => islandGenerator.sandTilemap,
        EdgeBlocker.Level.Grass => islandGenerator.grassTilemap,
        EdgeBlocker.Level.Mountain => islandGenerator.mountainTilemap,
        _ => null
    };
}