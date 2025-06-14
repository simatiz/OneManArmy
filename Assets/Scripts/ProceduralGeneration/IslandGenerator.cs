using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public float scale = 10f;
    public float islandRadius = 20f;
    public int minZoneSize = 5; // Мінімальна кількість тайлів, щоб зона могла бути поєднана мостом
    public int maxBridgeAttempts = 150; // Обмеження на кількість спроб знайти міст

    public bool spawnGrass = true;
    public bool spawnMountain = true;

    public Tilemap waterTilemap;
    public Tilemap foamTilemap;

    public Tilemap sandTilemap;
    public Tilemap grassTilemap;
    public Tilemap mountainTilemap;

    public Tilemap wallTilemap;
    public Tilemap higherWallTilemap;
    public Tilemap grassLadderTilemap;
    public Tilemap mountainLadderTilemap;

    public Tilemap grassWallShadowTilemap;
    public Tilemap sandWallShadowTilemap;
    public Tilemap bridgeShadowTilemap;

    public Tilemap grassDirtTilemap;
    public Tilemap sandDirtTilemap;

    public Tilemap sandBridgeTilemap;
    public Tilemap grassBridgeTilemap;
    public Tilemap mountainBridgeTilemap;

    public RuleTile waterTile;
    public RuleTile sandTile;
    public RuleTile grassTile;
    public RuleTile mountainTile;
    public RuleTile wallTile;
    public RuleTile bridgeTile;
    public RuleTile ladderTile;

    public TileBase shadowTile;
    public TileBase bridgeShadowTile;
    public TileBase grassDirtTile;
    public TileBase sandDirtTile;

    public AnimatedTile foamTile;

    public GameObject sandEdgeColliderPrefab;
    public GameObject grassEdgeColliderPrefab;
    public GameObject mountainEdgeColliderPrefab;

    public Transform sandColliderParent;
    public Transform grassColliderParent;
    public Transform mountainColliderParent;

    public Transform sandBridgeColliderParent;
    public Transform grassBridgeColliderParent;
    public Transform mountainBridgeColliderParent;

    public System.Action OnGenerationComplete;

    private float offsetX;
    private float offsetY;

    public bool generationCompleted = false;

    private HashSet<Vector3Int> placedBridges = new HashSet<Vector3Int>(); // Щоб уникнути дублювання мостів
    private HashSet<Vector3Int> placedWalls = new HashSet<Vector3Int>();
    private List<HashSet<Vector3Int>> sandIslands = new List<HashSet<Vector3Int>>();

    void Start()
    {
        // Генеруємо випадковий зсув, щоб кожен раз був унікальний острів
        offsetX = Random.Range(0f, 10000f);
        offsetY = Random.Range(0f, 10000f);

        if (!spawnGrass) spawnMountain = false;

        GenerateIsland();
    }

    void GenerateIsland()
    {
        // Отримуємо поточну позицію камери
        Vector3 cameraPosition = Camera.main.transform.position;

        // Обчислюємо центр острова відносно камери
        int centerX = Mathf.RoundToInt(cameraPosition.x);
        int centerY = Mathf.RoundToInt(cameraPosition.y);

        // Центруємо острів відносно камери
        Vector2 center = new Vector2(centerX, centerY);

        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int tilePosition = new Vector3Int(centerX + x, centerY + y, 0);
                Vector3Int aboveTilePosition = new Vector3Int(centerX + x, centerY + y + 1, 0);

                float distance = Vector2.Distance(new Vector2(tilePosition.x, tilePosition.y), center) / islandRadius;
                float noiseValue = Mathf.PerlinNoise((tilePosition.x + offsetX) / scale, (tilePosition.y + offsetY) / scale) - distance;

                // Очищаємо старі тайли перед встановленням нових
                waterTilemap.SetTile(tilePosition, null);
                sandTilemap.SetTile(tilePosition, null);
                grassTilemap.SetTile(tilePosition, null);
                mountainTilemap.SetTile(tilePosition, null);
                wallTilemap.SetTile(tilePosition, null);
                higherWallTilemap.SetTile(tilePosition, null);

                // Встановлення тайлів на відповідні рівні
                if (noiseValue <= 0.002f)
                {
                    waterTilemap.SetTile(tilePosition, waterTile);
                }
                else if (noiseValue <= 0.2f)
                {
                    waterTilemap.SetTile(tilePosition, waterTile);
                    sandTilemap.SetTile(tilePosition, sandTile);
                    foamTilemap.SetTile(tilePosition, foamTile);
                }
                else if (noiseValue <= 0.4f)
                {
                    waterTilemap.SetTile(tilePosition, waterTile);
                    sandTilemap.SetTile(tilePosition, sandTile);
                    if (spawnGrass)
                        grassTilemap.SetTile(tilePosition, grassTile);
                }
                else
                {
                    waterTilemap.SetTile(tilePosition, waterTile);
                    sandTilemap.SetTile(tilePosition, sandTile);
                    if (spawnGrass)
                        grassTilemap.SetTile(tilePosition, grassTile);
                    if (spawnMountain)
                        mountainTilemap.SetTile(tilePosition, mountainTile);
                }
            }
        }

        GenerateBridges();
        GenerateWalls();

        PlaceEdgeColliders(sandTilemap, sandBridgeTilemap, sandEdgeColliderPrefab, sandColliderParent);
        PlaceEdgeColliders(grassTilemap, grassBridgeTilemap, grassEdgeColliderPrefab, grassColliderParent);
        PlaceEdgeColliders(mountainTilemap, mountainBridgeTilemap, mountainEdgeColliderPrefab, mountainColliderParent);

        PlaceSideBridgeColliders(sandTilemap, sandBridgeTilemap, sandBridgeColliderParent, sandEdgeColliderPrefab);
        PlaceSideBridgeColliders(grassTilemap, grassBridgeTilemap, grassBridgeColliderParent, grassEdgeColliderPrefab);
        PlaceSideBridgeColliders(mountainTilemap, mountainBridgeTilemap, mountainBridgeColliderParent, mountainEdgeColliderPrefab);

        generationCompleted = true;
        OnGenerationComplete?.Invoke();
    }

    void GenerateWalls()
    {
        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int tilePosition = new Vector3Int(
                    Mathf.RoundToInt(Camera.main.transform.position.x) + x,
                    Mathf.RoundToInt(Camera.main.transform.position.y) + y,
                    0);

                Vector3Int belowTilePosition = new Vector3Int(tilePosition.x, tilePosition.y - 1, 0);
                Vector3Int secondBelowTilePosition = new Vector3Int(tilePosition.x, tilePosition.y - 2, 0);
                Vector3Int leftTilePosition = new Vector3Int(tilePosition.x - 1, tilePosition.y, 0);
                Vector3Int rightTilePosition = new Vector3Int(tilePosition.x + 1, tilePosition.y, 0);

                bool isGrassLeft = grassTilemap.GetTile(leftTilePosition) != null;
                bool isGrassRight = grassTilemap.GetTile(rightTilePosition) != null;

                bool isSandBelow = sandTilemap.GetTile(secondBelowTilePosition) != null;
                bool isGrassBelow = grassTilemap.GetTile(secondBelowTilePosition) != null;

                // Якщо в цій позиції є гора (mountainTile), ставимо звичайну стіну
                if (mountainTilemap.GetTile(tilePosition) != null && mountainTilemap.GetTile(belowTilePosition) == null)
                {
                    wallTilemap.SetTile(belowTilePosition, wallTile);
                    placedWalls.Add(belowTilePosition);

                    grassWallShadowTilemap.SetTile(belowTilePosition, shadowTile);
                    if (isSandBelow || isGrassBelow)
                    {
                        if (Random.value < 0.6f)
                        {
                            grassDirtTilemap.SetTile(belowTilePosition, grassDirtTile);
                        }
                    }
                }
                // Якщо в цій позиції є земля (grassTile), ставимо зміщену стіну
                else if (grassTilemap.GetTile(tilePosition) != null && grassTilemap.GetTile(belowTilePosition) == null)
                {
                    higherWallTilemap.SetTile(belowTilePosition, wallTile);
                    placedWalls.Add(belowTilePosition);

                    // Якщо зліва є трава, додаємо стіну ліворуч
                    if (isGrassLeft)
                    {
                        Vector3Int leftWallPosition = new Vector3Int(belowTilePosition.x - 1, belowTilePosition.y, 0);
                        higherWallTilemap.SetTile(leftWallPosition, wallTile);
                    }

                    // Якщо справа є трава, додаємо стіну праворуч
                    if (isGrassRight)
                    {
                        Vector3Int rightWallPosition = new Vector3Int(belowTilePosition.x + 1, belowTilePosition.y, 0);
                        higherWallTilemap.SetTile(rightWallPosition, wallTile);
                    }

                    sandWallShadowTilemap.SetTile(belowTilePosition, shadowTile);

                    if (isSandBelow || isGrassBelow)
                    {
                        if (Random.value < 0.6f)
                        {
                            sandDirtTilemap.SetTile(belowTilePosition, sandDirtTile);
                        }
                    }
                    else
                    {
                        sandTilemap.SetTile(belowTilePosition, null);
                    }
                }
            }
        }

        foreach (Vector3Int pos in placedWalls)
        {
            Vector3Int belowTilePosition = new Vector3Int(pos.x, pos.y - 1, 0);
            Vector3Int secondBelowTilePosition = new Vector3Int(pos.x, pos.y - 2, 0);
            Vector3Int aboveTilePosition = new Vector3Int(pos.x, pos.y + 1, 0);

            bool leftSupport = wallTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) != null ||
                higherWallTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) != null ||
                grassLadderTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) != null ||
                mountainLadderTilemap.GetTile(new Vector3Int(pos.x - 1, pos.y, 0)) != null;

            bool rightSupport = wallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) != null ||
                higherWallTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) != null ||
                grassLadderTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) != null ||
                mountainLadderTilemap.GetTile(new Vector3Int(pos.x + 1, pos.y, 0)) != null;

            bool isCorrectPlaceLadder =
                sandTilemap.GetTile(secondBelowTilePosition) != null &&
                sandTilemap.GetTile(belowTilePosition) != null &&
                higherWallTilemap.GetTile(belowTilePosition) == null &&
                higherWallTilemap.GetTile(secondBelowTilePosition) == null &&
                grassLadderTilemap.GetTile(belowTilePosition) == null &&
                grassLadderTilemap.GetTile(secondBelowTilePosition) == null &&
                sandBridgeTilemap.GetTile(aboveTilePosition) == null &&
                sandBridgeTilemap.GetTile(pos) == null &&
                grassBridgeTilemap.GetTile(aboveTilePosition) == null &&
                grassBridgeTilemap.GetTile(pos) == null &&
                mountainBridgeTilemap.GetTile(aboveTilePosition) == null &&
                mountainBridgeTilemap.GetTile(pos) == null &&
                leftSupport && rightSupport;

            if (Random.value < 0.5 && isCorrectPlaceLadder)
            {
                if (wallTilemap.HasTile(pos))
                {
                    wallTilemap.SetTile(pos, null);
                    mountainLadderTilemap.SetTile(pos, ladderTile);
                }
                else
                {
                    higherWallTilemap.SetTile(pos, null);
                    grassLadderTilemap.SetTile(pos, ladderTile);
                }
            }
        }

        List<Vector3Int> toRelocate = new List<Vector3Int>();

        for (int i = 1; i <= 3; i++) {
            toRelocate = new List<Vector3Int>();

            // Знаходимо всі стіни без опори знизу
            foreach (Vector3Int pos in placedWalls)
            {
                Vector3Int below = pos + Vector3Int.down;

                Vector3Int down = pos + Vector3Int.down;
                Vector3Int downLeft = pos + new Vector3Int(-1, -1, 0);
                Vector3Int downRight = pos + new Vector3Int(1, -1, 0);

                bool hasSupportBelow =
                    HasGroundTile(down) &&
                    HasGroundTile(downLeft) &&
                    HasGroundTile(downRight);

                if (!hasSupportBelow)
                {
                    toRelocate.Add(pos);
                }
            }

            // Переносимо стіни нижче, якщо це дозволено
            foreach (var wallPos in toRelocate)
            {
                Vector3Int above = wallPos + Vector3Int.up;
                Vector3Int newWallPos = wallPos + Vector3Int.down;

                // Пропускаємо, якщо над стіною — пісок (його чіпати не можна)
                if (sandTilemap.HasTile(above))
                    continue;

                // Перевіряємо, що зверху трава або гора
                bool isGrass = grassTilemap.HasTile(above);
                bool isMountain = mountainTilemap.HasTile(above);

                if (!isGrass && !isMountain)
                    continue;

                // Видаляємо стару стіну
                if (wallTilemap.HasTile(wallPos)) wallTilemap.SetTile(wallPos, null);
                if (higherWallTilemap.HasTile(wallPos)) higherWallTilemap.SetTile(wallPos, null);

                // Видаляємо блок над нею
                if (isGrass) grassTilemap.SetTile(above, null);
                if (isMountain) mountainTilemap.SetTile(above, null);

                // Ставимо нову стіну нижче
                if (!wallTilemap.HasTile(newWallPos) && !higherWallTilemap.HasTile(newWallPos))
                {
                    wallTilemap.SetTile(newWallPos, wallTile);
                    placedWalls.Add(newWallPos);
                }
            }
        }

        EnsureSandUnderGrassWalls();

        // Зміщуємо Tilemap нижчих стін вверх
        higherWallTilemap.transform.position += new Vector3(0, +0.05f, 0);
        grassLadderTilemap.transform.position += new Vector3(0, +0.05f, 0);
        sandDirtTilemap.transform.position += new Vector3(0, +0.05f, 0);
        grassWallShadowTilemap.transform.position += new Vector3(0, -0.02f, 0);
        sandWallShadowTilemap.transform.position += new Vector3(0, -0.02f, 0);
    }

    bool HasGroundTile(Vector3Int position)
    {
        // Якщо поза межами — не опора
        if (!waterTilemap.cellBounds.Contains(position)) return false;

        if (waterTilemap.HasTile(position) || higherWallTilemap.HasTile(position) || grassLadderTilemap.HasTile(position)) return false;

        return sandTilemap.HasTile(position) ||
               grassTilemap.HasTile(position) ||
               mountainTilemap.HasTile(position);
    }

    void EnsureSandUnderGrassWalls()
    {
        foreach (var pos in placedWalls)
        {
            Vector3Int above = pos + Vector3Int.up;

            if (grassTilemap.HasTile(above))
            {
                Vector3Int below1 = pos + Vector3Int.down;
                Vector3Int below2 = below1 + Vector3Int.down;
                Vector3Int downLeft = pos + new Vector3Int(-1, -1, 0);
                Vector3Int downRight = pos + new Vector3Int(1, -1, 0);

                PlaceSand(pos);
                PlaceSand(below1);
                PlaceSand(below2);
                PlaceSand(downLeft);
                PlaceSand(downRight);
            }
        }
    }

    void PlaceSand(Vector3Int pos)
    {
        if (!sandTilemap.HasTile(pos))
        {
            sandTilemap.SetTile(pos, sandTile);
            foamTilemap.SetTile(pos, foamTile);
        }
    }

    void GenerateBridges()
    {
        ConnectZones(grassTilemap);
        sandIslands = ConnectZones(sandTilemap);
        ConnectZones(mountainTilemap);
    }

    List<HashSet<Vector3Int>> ConnectZones(Tilemap terrainTilemap)
    {
        List<HashSet<Vector3Int>> regions = FindDisconnectedRegions(terrainTilemap);

        List<HashSet<Vector3Int>> islands = new List<HashSet<Vector3Int>>();
        foreach (var hashSet in regions)
        {
            islands.Add(new HashSet<Vector3Int>(hashSet));
        }

        // Видаляємо маленькі зони, які не потрібно поєднувати
        regions.RemoveAll(region => region.Count < minZoneSize);
        if (regions.Count <= 1) return new List<HashSet<Vector3Int>>(); // Всі зони вже пов'язані

        int attempts = 0;

        while (regions.Count > 1 && attempts < maxBridgeAttempts)
        {
            attempts++;

            HashSet<Vector3Int> regionA = regions[0];
            HashSet<Vector3Int> closestRegion = null;
            Vector3Int bestStart = Vector3Int.zero, bestEnd = Vector3Int.zero;
            int bestDistance = int.MaxValue;

            foreach (var regionB in regions)
            {
                if (regionA == regionB) continue;

                foreach (var posA in regionA)
                {
                    foreach (var posB in regionB)
                    {
                        int distanceX = Mathf.Abs(posA.x - posB.x);
                        int distanceY = Mathf.Abs(posA.y - posB.y);

                        // Міст тільки горизонтальний або вертикальний
                        if (distanceX > 0 && distanceY > 0) continue;

                        int distance = distanceX + distanceY;

                        if (distance < bestDistance && CanPlaceBridge(posA, posB))
                        {
                            bestDistance = distance;
                            bestStart = posA;
                            bestEnd = posB;
                            closestRegion = regionB;
                        }
                    }
                }
            }

            if (closestRegion != null)
            {
                PlaceBridge(bestStart, bestEnd);
                regionA.UnionWith(closestRegion);
                regions.Remove(closestRegion);
            }
            else
            {
                // Якщо після кількох спроб не знайшлося хорошого мосту – виходимо
                break;
            }
        }
        return islands;
    }

    List<HashSet<Vector3Int>> FindDisconnectedRegions(Tilemap terrainTilemap)
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        List<HashSet<Vector3Int>> regions = new List<HashSet<Vector3Int>>();

        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int pos = new Vector3Int(
                    Mathf.RoundToInt(Camera.main.transform.position.x) + x,
                    Mathf.RoundToInt(Camera.main.transform.position.y) + y,
                    0
                );

                if (terrainTilemap.GetTile(pos) != null && !visited.Contains(pos))
                {
                    HashSet<Vector3Int> region = new HashSet<Vector3Int>();
                    Queue<Vector3Int> queue = new Queue<Vector3Int>();
                    queue.Enqueue(pos);
                    visited.Add(pos);

                    while (queue.Count > 0)
                    {
                        Vector3Int current = queue.Dequeue();
                        region.Add(current);

                        foreach (Vector3Int neighbor in GetNeighbors(current))
                        {
                            if (!visited.Contains(neighbor) && terrainTilemap.GetTile(neighbor) != null)
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }

                    regions.Add(region);
                }
            }
        }

        return regions;
    }

    bool CanPlaceBridge(Vector3Int start, Vector3Int end)
    {
        if (placedBridges.Contains(start) || placedBridges.Contains(end)) return false;

        Vector3Int direction = new Vector3Int(
            Mathf.Clamp(end.x - start.x, -1, 1),
            Mathf.Clamp(end.y - start.y, -1, 1),
            0
        );

        Vector3Int pos = start;

        // Перевіряємо, чи поруч із початком або кінцем мосту є інший міст
        if (IsBridgeNearStartOrEnd(start) || IsBridgeNearStartOrEnd(end)) return false;

        while (pos != end)
        {
            pos += direction;
            // Перевірка чи вже є міст у цьому місці
            if (sandBridgeTilemap.GetTile(pos) != null) return false;
            if (grassBridgeTilemap.GetTile(pos) != null) return false;
            if (mountainBridgeTilemap.GetTile(pos) != null) return false;
            if (IsBridgeTooClose(pos, direction)) return false;
            if (waterTilemap.GetTile(pos) == null) return false;
        }

        return true;
    }

    bool IsBridgeTooClose(Vector3Int pos, Vector3Int direction)
    {
        // Перевіряємо сусідні тайли в напрямку, перпендикулярному мосту
        Vector3Int checkLeft = pos + new Vector3Int(-direction.y, direction.x, 0);
        Vector3Int checkRight = pos + new Vector3Int(direction.y, -direction.x, 0);

        return (sandBridgeTilemap.GetTile(checkLeft) != null || sandBridgeTilemap.GetTile(checkRight) != null ||
            grassBridgeTilemap.GetTile(checkLeft) != null || grassBridgeTilemap.GetTile(checkRight) != null ||
            mountainBridgeTilemap.GetTile(checkLeft) != null || mountainBridgeTilemap.GetTile(checkRight) != null);
    }

    bool IsBridgeNearStartOrEnd(Vector3Int pos)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
    {
        pos + Vector3Int.up,
        pos + Vector3Int.down,
        pos + Vector3Int.left,
        pos + Vector3Int.right
    };

        foreach (var neighbor in neighbors)
        {
            if (sandBridgeTilemap.GetTile(neighbor) != null || grassBridgeTilemap.GetTile(neighbor) != null || mountainBridgeTilemap.GetTile(neighbor) != null)
            {
                return true; // Є міст поруч із початком або кінцем
            }
        }

        return false;
    }

    void PlaceBridge(Vector3Int start, Vector3Int end)
    {
        Vector3Int direction = new Vector3Int(
            Mathf.Clamp(end.x - start.x, -1, 1),
            Mathf.Clamp(end.y - start.y, -1, 1), 0);

        Vector3Int pos = start;

        bool connectsHigh = IsGrassTile(start) && IsGrassTile(end);
        bool connectsHighest = IsMountainTile(start) && IsMountainTile(end);

        if (!placedBridges.Contains(pos))
        {
            Tilemap targetBridgeMap = GetBridgeTilemapForLevel(start, end);
            if (targetBridgeMap != null)
            {
                targetBridgeMap.SetTile(pos, bridgeTile);
                placedBridges.Add(pos);
            }
        }

        while (pos != end)
        {
            pos += direction;
            if (!placedBridges.Contains(pos))
            {
                Tilemap targetBridgeMap = GetBridgeTilemapForLevel(start, end);
                if (targetBridgeMap != null)
                {
                    targetBridgeMap.SetTile(pos, bridgeTile);
                    placedBridges.Add(pos);
                }
                if (connectsHigh && direction.y == 0 && pos != start && pos != end)
                    AddBridgeShadow(pos);
            }
        }

        Tilemap GetBridgeTilemapForLevel(Vector3Int start, Vector3Int end)
        {
            bool isSand = sandTilemap.GetTile(start) != null && sandTilemap.GetTile(end) != null;
            bool isGrass = grassTilemap.GetTile(start) != null && grassTilemap.GetTile(end) != null;
            bool isMountain = mountainTilemap.GetTile(start) != null && mountainTilemap.GetTile(end) != null;

            if (isMountain) return mountainBridgeTilemap;
            if (isGrass) return grassBridgeTilemap;
            if (isSand) return sandBridgeTilemap;

            return null;
        }

        void AddBridgeShadow(Vector3Int bridgePos)
        {
            Vector3Int shadowPos = bridgePos + Vector3Int.down; // Тінь завжди нижче моста

            // Якщо тінь ще не існує, додаємо її
            if (bridgeShadowTilemap.GetTile(shadowPos) == null)
            {
                if (connectsHigh && !connectsHighest)
                {
                    if (!IsGrassTile(shadowPos) && !IsMountainTile(shadowPos))
                    {
                        bridgeShadowTilemap.SetTile(shadowPos, bridgeShadowTile);
                    }
                }
                else
                {
                    if (!IsMountainTile(shadowPos))
                    {
                        if (IsGrassTile(shadowPos))
                            bridgeShadowTilemap.SetTile(shadowPos, bridgeShadowTile);
                        else
                            if (!IsGrassTile(shadowPos + Vector3Int.down))
                            bridgeShadowTilemap.SetTile(shadowPos + Vector3Int.down, bridgeShadowTile);
                    }
                }
            }
        }
    }

    void PlaceEdgeColliders(Tilemap tilemap, Tilemap bridgeTilemap, GameObject colliderPrefab, Transform parent)
    {
        HashSet<Vector3Int> placed = new HashSet<Vector3Int>(); // Щоб уникнути дублювання

        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int pos = new Vector3Int(
                    Mathf.RoundToInt(Camera.main.transform.position.x) + x,
                    Mathf.RoundToInt(Camera.main.transform.position.y) + y,
                    0);

                if (tilemap.GetTile(pos) != null)
                {
                    foreach (var neighbor in GetNeighbors(pos))
                    {
                        if (
                            tilemap.GetTile(neighbor) == null &&
                            grassLadderTilemap.GetTile(neighbor) == null &&
                            mountainLadderTilemap.GetTile(neighbor) == null &&
                            !placed.Contains(neighbor))
                        {
                            Vector3 worldPos = tilemap.CellToWorld(neighbor) + new Vector3(0.3f, 0.3f, 0);
                            Instantiate(colliderPrefab, worldPos, Quaternion.identity, parent);
                            placed.Add(neighbor);
                        }
                    }
                }
            }
        }
    }

    void PlaceSideBridgeColliders(Tilemap groundTilemap, Tilemap bridgeMap, Transform colliderParent, GameObject colliderPrefab)
    {
        HashSet<Vector3Int> placed = new HashSet<Vector3Int>();

        foreach (Vector3Int pos in bridgeMap.cellBounds.allPositionsWithin)
        {
            if (!bridgeMap.HasTile(pos)) continue;

            bool hasLeft = bridgeMap.HasTile(pos + Vector3Int.left);
            bool hasRight = bridgeMap.HasTile(pos + Vector3Int.right);
            bool hasUp = bridgeMap.HasTile(pos + Vector3Int.up);
            bool hasDown = bridgeMap.HasTile(pos + Vector3Int.down);

            // Якщо горизонтальний міст
            if (hasLeft && hasRight && !(hasUp || hasDown))
            {
                Vector3Int up = pos + Vector3Int.up;
                Vector3Int down = pos + Vector3Int.down;

                if (!placed.Contains(up))
                {
                    Vector3 worldUp = bridgeMap.CellToWorld(up) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldUp, Quaternion.identity, colliderParent);
                    placed.Add(up);
                }

                if (!placed.Contains(down))
                {
                    Vector3 worldDown = bridgeMap.CellToWorld(down) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldDown, Quaternion.identity, colliderParent);
                    placed.Add(down);
                }
            }

            if ((hasLeft || hasRight) && !(hasUp || hasDown))
            {
                Vector3Int up = pos + Vector3Int.up;
                Vector3Int down = pos + Vector3Int.down;

                if (!placed.Contains(up) && !groundTilemap.HasTile(up))
                {
                    Vector3 worldUp = bridgeMap.CellToWorld(up) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldUp, Quaternion.identity, colliderParent);
                    placed.Add(up);
                }

                if (!placed.Contains(down) && !groundTilemap.HasTile(down))
                {
                    Vector3 worldDown = bridgeMap.CellToWorld(down) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldDown, Quaternion.identity, colliderParent);
                    placed.Add(down);
                }
            }

            // Якщо вертикальний міст
            if (hasUp && hasDown && !(hasLeft || hasRight))
            {
                Vector3Int left = pos + Vector3Int.left;
                Vector3Int right = pos + Vector3Int.right;

                if (!placed.Contains(left))
                {
                    Vector3 worldLeft = bridgeMap.CellToWorld(left) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldLeft, Quaternion.identity, colliderParent);
                    placed.Add(left);
                }

                if (!placed.Contains(right))
                {
                    Vector3 worldRight = bridgeMap.CellToWorld(right) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldRight, Quaternion.identity, colliderParent);
                    placed.Add(right);
                }
            }

            if ((hasUp || hasDown) && !(hasLeft || hasRight))
            {
                Vector3Int left = pos + Vector3Int.left;
                Vector3Int right = pos + Vector3Int.right;

                if (!placed.Contains(left) && !groundTilemap.HasTile(left))
                {
                    Vector3 worldLeft = bridgeMap.CellToWorld(left) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldLeft, Quaternion.identity, colliderParent);
                    placed.Add(left);
                }

                if (!placed.Contains(right) && !groundTilemap.HasTile(right))
                {
                    Vector3 worldRight = bridgeMap.CellToWorld(right) + new Vector3(0.3f, 0.3f, 0);
                    Instantiate(colliderPrefab, worldRight, Quaternion.identity, colliderParent);
                    placed.Add(right);
                }
            }
        }
    }

    bool IsGrassTile(Vector3Int pos)
    {
        return grassTilemap.GetTile(pos) != null;
    }

    bool IsMountainTile(Vector3Int pos)
    {
        return mountainTilemap.GetTile(pos) != null;
    }

    public List<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        return new List<Vector3Int>
        {
            pos + Vector3Int.up,
            pos + Vector3Int.down,
            pos + Vector3Int.left,
            pos + Vector3Int.right
        };
    }

    public List<HashSet<Vector3Int>> getSandIslands()
    {
        return sandIslands;
    }
}