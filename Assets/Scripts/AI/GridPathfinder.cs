using System;
using System.Collections.Generic;
using UnityEngine;

public class PathOptions
{
    public Func<Tile, bool> CanTraverse;
    public Func<Tile, int> TileCost;

    public static PathOptions Default => new PathOptions
    {
        CanTraverse = tile => tile != null && tile.IsWalkable,
        TileCost = tile => 1
    };

    public static PathOptions CreateHintDefaults()
    {
        return new PathOptions
        {
            CanTraverse = tile =>
            {
                if (tile == null || !tile.IsWalkable) return false;
                if (tile is FireTile || tile is GlassTile) return false;
                return true;
            },
            TileCost = tile =>
            {
                if (tile is StickyTile sticky) return 1 + Mathf.Max(1, sticky.ExtraCommandCost);
                if (tile is IceTile) return 2;
                return 1;
            }
        };
    }

    public static PathOptions CreateChaserDefaults()
    {
        return Default;
    }
}

public static class GridPathfinder
{
    class Node
    {
        public Vector2Int Pos;
        public int G;
        public int H;
        public Node Parent;
        public int F => G + H;
    }

    public static bool TryFindPath(GridManager grid, Vector2Int start, Vector2Int goal, PathOptions options, out List<Vector2Int> path, out int totalCost)
    {
        path = null;
        totalCost = 0;
        if (grid == null) return false;
        if (start == goal)
        {
            path = new List<Vector2Int> { start };
            return true;
        }

        options ??= PathOptions.Default;
        var open = new List<Node>();
        var nodes = new Dictionary<Vector2Int, Node>();
        var gScores = new Dictionary<Vector2Int, int> { [start] = 0 };
        var closed = new HashSet<Vector2Int>();

        Node startNode = new Node { Pos = start, G = 0, H = Heuristic(start, goal) };
        open.Add(startNode);
        nodes[start] = startNode;

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.F.CompareTo(b.F));
            var current = open[0];
            open.RemoveAt(0);

            if (current.Pos == goal)
            {
                path = Reconstruct(current);
                totalCost = current.G;
                return true;
            }

            closed.Add(current.Pos);

            foreach (var neighbor in GetNeighbors(current.Pos))
            {
                if (closed.Contains(neighbor)) continue;
                if (!grid.TryGetTile(neighbor, out var neighborTile)) continue;
                if (options.CanTraverse != null && !options.CanTraverse(neighborTile)) continue;

                int tentativeG = current.G + Math.Max(1, options.TileCost?.Invoke(neighborTile) ?? 1);

                if (!gScores.TryGetValue(neighbor, out var existingG) || tentativeG < existingG)
                {
                    gScores[neighbor] = tentativeG;
                    if (!nodes.TryGetValue(neighbor, out var neighborNode))
                    {
                        neighborNode = new Node { Pos = neighbor };
                        nodes[neighbor] = neighborNode;
                    }

                    neighborNode.G = tentativeG;
                    neighborNode.H = Heuristic(neighbor, goal);
                    neighborNode.Parent = current;

                    if (!open.Contains(neighborNode))
                    {
                        open.Add(neighborNode);
                    }
                }
            }
        }

        return false;
    }

    static IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        yield return pos + Vector2Int.up;
        yield return pos + Vector2Int.down;
        yield return pos + Vector2Int.left;
        yield return pos + Vector2Int.right;
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    static List<Vector2Int> Reconstruct(Node node)
    {
        var result = new List<Vector2Int>();
        var current = node;
        while (current != null)
        {
            result.Add(current.Pos);
            current = current.Parent;
        }
        result.Reverse();
        return result;
    }
}
