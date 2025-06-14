using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimpleGridPathfinder : MonoBehaviour
{
    [SerializeField] private Tilemap sandTilemap;
    [SerializeField] private Tilemap grassTilemap;
    [SerializeField] private Tilemap mountainTilemap;

    [SerializeField] private Tilemap[] blockers;

    public EdgeBlocker.Level level;

    public void Initialize(
        Tilemap sand, Tilemap grass, Tilemap mountain,
        Tilemap[] blockingMaps, EdgeBlocker.Level allowed)
    {
        sandTilemap = sand;
        grassTilemap = grass;
        mountainTilemap = mountain;
        blockers = blockingMaps;
        level = allowed;
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Tilemap map = GetLevelMap(level);
        if (map == null) return null;

        Vector3Int start = map.WorldToCell(startWorld);
        Vector3Int goal = map.WorldToCell(targetWorld);

        if (!IsTopmostTile(start) || !IsTopmostTile(goal)) return null;

        /* A* */
        List<Node> open = new();                         // відкрита множина
        HashSet<Vector3Int> closed = new();              // закрита множина
        Dictionary<Vector3Int, Node> all = new();

        Node startNode = new Node(start, 0, Heuristic(start, goal));
        open.Add(startNode); all[start] = startNode;

        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down,
                              Vector3Int.left, Vector3Int.right };

        while (open.Count > 0)
        {
            Node cur = PopLowestF(open);
            if (cur.pos == goal) return Reconstruct(cur, map);

            open.Remove(cur); closed.Add(cur.pos);

            foreach (Vector3Int d in dirs)
            {
                Vector3Int nb = cur.pos + d;
                if (closed.Contains(nb)) continue;
                if (!IsTopmostTile(nb)) continue;
                if (IsBlocked(nb)) continue;

                int g = cur.g + 1;
                if (!all.TryGetValue(nb, out Node n))
                {
                    n = new Node(nb, g, Heuristic(nb, goal));
                    n.parent = cur;
                    all[nb] = n;
                    open.Add(n);
                }
                else if (g < n.g)
                {
                    n.g = g;
                    n.parent = cur;
                }
            }
        }
        return null;    // шлях не знайдено
    }

    public bool IsTopmostTile(Vector3Int c)
    {
        switch (level)
        {
            case EdgeBlocker.Level.Sand:
                return sandTilemap.HasTile(c) &&
                       !grassTilemap.HasTile(c) &&
                       !mountainTilemap.HasTile(c);

            case EdgeBlocker.Level.Grass:
                return grassTilemap.HasTile(c) &&
                       !mountainTilemap.HasTile(c);

            case EdgeBlocker.Level.Mountain:
                return mountainTilemap.HasTile(c);
        }
        return false;
    }

    Tilemap GetLevelMap(EdgeBlocker.Level l) => l switch
    {
        EdgeBlocker.Level.Sand => sandTilemap,
        EdgeBlocker.Level.Grass => grassTilemap,
        EdgeBlocker.Level.Mountain => mountainTilemap,
        _ => null
    };

    bool IsBlocked(Vector3Int c)
    {
        foreach (var b in blockers)
            if (b != null && b.HasTile(c)) return true;
        return false;
    }

    int Heuristic(Vector3Int a, Vector3Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    Node PopLowestF(List<Node> list)
    {
        Node best = list[0];
        foreach (var n in list)
            if (n.F < best.F || (n.F == best.F && n.h < best.h)) best = n;
        return best;
    }

    List<Vector3> Reconstruct(Node n, Tilemap map)
    {
        List<Vector3> p = new();
        while (n.parent != null)
        {
            p.Add(map.GetCellCenterWorld(n.pos));
            n = n.parent;
        }
        p.Reverse();
        return p;
    }

    class Node
    {
        public Vector3Int pos;
        public int g, h;              // g-cost, h-cost
        public int F => g + h;
        public Node parent;
        public Node(Vector3Int p, int g, int h)
        { pos = p; this.g = g; this.h = h; }
    }
}