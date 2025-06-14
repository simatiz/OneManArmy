using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public IslandGenerator islandGenerator;

    public List<ItemSpawnData> sandItems;
    public List<ItemSpawnData> grassItems;
    public List<ItemSpawnData> mountainItems;

    public Transform itemParent;

    void Start()
    {
        if (islandGenerator == null)
        {
            Debug.LogError("IslandGenerator не встановлено!");
            return;
        }

        islandGenerator.OnGenerationComplete += () =>
        {
            StartCoroutine(DelayedSpawn());
        };
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(0.1f);

        SpawnItemsOnTilemap(islandGenerator.sandTilemap, sandItems, new Tilemap[] { islandGenerator.grassTilemap, islandGenerator.mountainTilemap }, islandGenerator.wallTilemap, islandGenerator.higherWallTilemap);
        SpawnItemsOnTilemap(islandGenerator.grassTilemap, grassItems, new Tilemap[] { islandGenerator.mountainTilemap }, islandGenerator.wallTilemap, islandGenerator.higherWallTilemap);
        SpawnItemsOnTilemap(islandGenerator.mountainTilemap, mountainItems, null, islandGenerator.wallTilemap, islandGenerator.higherWallTilemap);
    }

    void SpawnItemsOnTilemap(Tilemap tilemap, List<ItemSpawnData> itemsToSpawn, Tilemap[] blockedAbove, Tilemap wallTilemap, Tilemap ladderTilemap)
    {
        List<Vector3Int> validTiles = new List<Vector3Int>();

        tilemap.CompressBounds();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            // Перевірка: не має бути стіни чи драбини
            if ((wallTilemap != null && wallTilemap.HasTile(pos)) || (ladderTilemap != null && ladderTilemap.HasTile(pos)))
                continue;

            // Блок над рівнем
            bool blocked = false;
            if (blockedAbove != null)
            {
                foreach (var above in blockedAbove)
                {
                    if (above.HasTile(pos))
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            if (!blocked)
                validTiles.Add(pos);
        }

        foreach (var itemData in itemsToSpawn)
        {
            for (int i = 0; i < itemData.count; i++)
            {
                if (validTiles.Count == 0) return;

                Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];
                Vector3 worldPos = tilemap.CellToWorld(spawnTile) + new Vector3(0.5f, 0.5f, 0);

                Instantiate(itemData.prefab, worldPos, Quaternion.identity, itemParent);
            }
        }
    }
}

[System.Serializable]
public class ItemSpawnData
{
    public GameObject prefab;
    public int count;
}