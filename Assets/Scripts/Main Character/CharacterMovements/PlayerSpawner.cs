using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public IslandGenerator islandGenerator;
    public GameObject playerPrefab;

    void Start()
    {
        if (islandGenerator == null)
        {
            Debug.LogError("IslandGenerator не встановлено! Перетягніть об'єкт у PlayerSpawner в інспекторі.");
            RestartLevel();
            return;
        }
        try
        {
            StartCoroutine(WaitForIslandGeneration());
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Помилка під час запуску генерації острова: " + ex.Message);
            RestartLevel();
        }
    }

    IEnumerator WaitForIslandGeneration()
    {
        yield return new WaitUntil(() => GetTileCount(islandGenerator.sandTilemap) > 0);

        Debug.Log("Острів згенеровано! Запускаємо пошук піщаних островів.");
        List<HashSet<Vector3Int>> sandIslands = islandGenerator.getSandIslands();

        if (sandIslands.Count > 0)
        {
            List<Vector3Int> islandTiles = FindChosen(sandIslands);
            // Фільтруємо лише внутрішні плитки (відкидаємо межі)
            List<Vector3Int> safeTiles = new List<Vector3Int>();
            foreach (Vector3Int tile in islandTiles)
            {
                bool isEdge = false;
                foreach (Vector3Int dir in new Vector3Int[] {
                new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0) })
                {
                    Vector3Int neighbor = tile + dir;
                    if (!islandGenerator.sandTilemap.HasTile(neighbor)) // Якщо хоча б один сусід не є піском, значить це край острова
                    {
                        isEdge = true;
                        break;
                    }
                }
                if (!isEdge)
                    safeTiles.Add(tile);
            }

            if (safeTiles.Count == 0)
            {
                Debug.LogWarning("Не знайдено внутрішніх тайлів для спавну!");
                RestartLevel();
                yield break;
            }

            SpawnPlayer(safeTiles[Random.Range(0, safeTiles.Count)]);
        }
        else
        {
            Debug.LogWarning("Не знайдено жодного піщаного острова для спавну гравця!");
            RestartLevel();
            yield break;
        }
    }

    int GetTileCount(Tilemap tilemap)
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        int count = 0;

        foreach (TileBase tile in allTiles)
        {
            if (tile != null) count++;
        }
        return count;
    }

    List<Vector3Int> FindChosen(List<HashSet<Vector3Int>> islands)
    {
        List<HashSet<Vector3Int>> validIslands = new List<HashSet<Vector3Int>>();

        foreach (var island in islands)
        {
            if (island.Count < 10 || island.Count > 30)
                continue;

            bool bridgeTouching = false;

            foreach (Vector3Int tile in island)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector3Int neighbor = tile + new Vector3Int(dx, dy, 0);
                        if (islandGenerator.sandBridgeTilemap.HasTile(neighbor))
                        {
                            bridgeTouching = true;
                            break;
                        }
                    }
                    if (bridgeTouching) break;
                }

                if (bridgeTouching)
                    break;
            }

            if (bridgeTouching)
            {
                validIslands.Add(island);
            }
        }

        if (validIslands.Count == 0)
        {
            Debug.LogWarning("Не знайдено жодного острова, що має міст.");
            RestartLevel();
            return new List<Vector3Int> { Vector3Int.zero }; // Запобігає крашу
        }

        HashSet<Vector3Int> chosenIsland = validIslands[Random.Range(0, validIslands.Count)];
        return new List<Vector3Int>(chosenIsland);
    }

    void SpawnPlayer(Vector3Int spawnTile)
    {
        if (spawnTile == Vector3Int.zero)
        {
            Debug.LogWarning("Спавн не відбувся, координати невірні!");
            RestartLevel();
            return;
        }

        Vector3 worldPosition = islandGenerator.sandTilemap.CellToWorld(spawnTile);
        playerPrefab.transform.position = worldPosition + new Vector3(0.8f, -0.5f, 0);
        Debug.Log($"Гравець заспавнений на {worldPosition}");
    }

    void RestartLevel()
    {
        Debug.LogWarning("Рестарт рівня через помилку спавну!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}