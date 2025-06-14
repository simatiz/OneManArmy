using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Links")]
    public IslandGenerator islandGenerator;
    public Transform enemyParent;

    [Header("Prefabs")]
    public GameObject[] sandEnemies;
    public GameObject[] grassEnemies;
    public GameObject[] mountainEnemies;

    [Header("Counts")]
    public int sandEnemyCount = 3;
    public int grassEnemyCount = 4;
    public int mountainEnemyCount = 2;
    static readonly Vector3 VISUAL_OFFSET = Vector3.down * 0.9f;

    void Start()
    {
        if (islandGenerator == null)
        {
            Debug.LogError("[EnemySpawner] IslandGenerator reference is missing");
            return;
        }

        islandGenerator.OnGenerationComplete += () => StartCoroutine(DelayedSpawn());
        if (islandGenerator.generationCompleted) StartCoroutine(DelayedSpawn());
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(0.4f);

        SpawnOnTilemap(
            islandGenerator.sandTilemap,
            sandEnemies,
            sandEnemyCount,
            EdgeBlocker.Level.Sand,
            new[] { islandGenerator.grassTilemap, islandGenerator.mountainTilemap });

        SpawnOnTilemap(
            islandGenerator.grassTilemap,
            grassEnemies,
            grassEnemyCount,
            EdgeBlocker.Level.Grass,
            new[] { islandGenerator.mountainTilemap });

        SpawnOnTilemap(
            islandGenerator.mountainTilemap,
            mountainEnemies,
            mountainEnemyCount,
            EdgeBlocker.Level.Mountain,
            null);
    }

    void SpawnOnTilemap(
        Tilemap levelMap,
        GameObject[] prefabs,
        int count,
        EdgeBlocker.Level level,
        Tilemap[] higherMaps)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        List<Vector3Int> candidates = CollectValidTiles(levelMap, level, higherMaps);
        if (candidates.Count == 0) return;

        HashSet<Vector3Int> used = new HashSet<Vector3Int>();

        for (int i = 0; i < count; i++)
        {
            Vector3Int cell;
            int safety = 50;
            do { cell = candidates[Random.Range(0, candidates.Count)]; } while (used.Contains(cell) && --safety > 0);
            used.Add(cell);

            Vector3 spawnPos = levelMap.GetCellCenterWorld(cell) + VISUAL_OFFSET;
            GameObject enemyGO = Instantiate(
                prefabs[Random.Range(0, prefabs.Length)],
                spawnPos,
                Quaternion.identity,
                enemyParent);

            if (enemyGO.TryGetComponent(out EnemyAI ai))
                ai.allowedLevel = level;
        }
    }

    List<Vector3Int> CollectValidTiles(
        Tilemap levelMap,
        EdgeBlocker.Level level,
        Tilemap[] higherMaps)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        levelMap.CompressBounds();

        foreach (Vector3Int pos in levelMap.cellBounds.allPositionsWithin)
        {
            if (!IsTopmostForLevel(pos, level, higherMaps)) continue;

            // блокери (стіни/драбини)
            if (islandGenerator.wallTilemap.HasTile(pos) ||
                islandGenerator.higherWallTilemap.HasTile(pos) ||
                islandGenerator.grassLadderTilemap.HasTile(pos) ||
                islandGenerator.mountainLadderTilemap.HasTile(pos))
                continue;

            list.Add(pos);
        }

        return list;
    }

    bool IsTopmostForLevel(Vector3Int cell, EdgeBlocker.Level level, Tilemap[] higherMaps)
    {
        // На клітинці ОБОВ’ЯЗКОВО має бути тайл потрібного рівня
        switch (level)
        {
            case EdgeBlocker.Level.Sand: if (!islandGenerator.sandTilemap.HasTile(cell)) return false; break;
            case EdgeBlocker.Level.Grass: if (!islandGenerator.grassTilemap.HasTile(cell)) return false; break;
            case EdgeBlocker.Level.Mountain: if (!islandGenerator.mountainTilemap.HasTile(cell)) return false; break;
        }

        // Переконатися, що над нею немає тайлів вищих шарів
        if (higherMaps != null)
            foreach (Tilemap t in higherMaps)
                if (t != null && t.HasTile(cell)) return false;

        return true;
    }
}