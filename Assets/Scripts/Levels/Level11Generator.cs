using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Level11Generator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GridManager grid;
    [SerializeField] PlayerController player;
    [SerializeField] UnityEvent onGenerationSucceeded;
    [SerializeField] Text seedLabel;

    [Header("Prefabs")]
    [SerializeField] Tile normalPrefab;
    [SerializeField] Tile goalPrefab;
    [SerializeField] Tile wallPrefab;
    [SerializeField] Tile icePrefab;
    [SerializeField] Tile risingPrefab;
    [SerializeField] Tile stickyPrefab;
    [SerializeField] Tile hazardFirePrefab;
    [SerializeField] Tile hazardGlassPrefab;
    [SerializeField] Tile springPrefab;

    [Header("Generation Settings")]
    [SerializeField, Range(5, 10)] int gridSize = 7;
    [SerializeField] bool autoGenerateOnStart = true;
    [SerializeField] int maxAttempts = 25;
    [SerializeField] int baseMoveCap = 12;
    [SerializeField] bool forceSlide = true;
    [SerializeField] bool resetOnFail = false;
    [SerializeField] bool alignGridToPlayer = true;
    [SerializeField] float wallChance = 0.15f;
    [SerializeField] float iceChance = 0.15f;
    [SerializeField] float risingChance = 0.12f;
    [SerializeField] float stickyChance = 0.08f;
    [SerializeField] float hazardChance = 0.05f;
    [SerializeField] float springChance = 0.05f;

    enum CellType { Normal, Start, Goal, Wall, Ice, Rising, Sticky, HazardFire, HazardGlass, Spring }

    readonly List<GameObject> spawnedTiles = new List<GameObject>();
    System.Random seededRandom;
    int lastSeed;

    void Start()
    {
        if (autoGenerateOnStart)
            GenerateNewSeed();
    }

    [ContextMenu("Generate New Level")]
    public void GenerateNewSeed()
    {
        lastSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        Generate(lastSeed);
    }

    public void Generate(int seed)
    {
        if (!grid || !player || !normalPrefab || !goalPrefab)
        {
            Debug.LogError("[Level11Generator] Missing references.");
            return;
        }

        Vector3 playerPos = player ? player.transform.position : Vector3.zero;

        seededRandom = new System.Random(seed);
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            ClearTiles();
            var layout = BuildLayout(seed + attempt, out var start, out var goal);

            if (alignGridToPlayer && grid)
            {
                var desiredStartWorld = new Vector3(playerPos.x, grid.transform.position.y, playerPos.z);
                var currentStartWorld = grid.WorldFromGrid(start, grid.transform.position.y);
                var offset = desiredStartWorld - currentStartWorld;
                grid.transform.position += offset;
                if (grid.TilesRoot)
                {
                    grid.TilesRoot.localPosition = Vector3.zero;
                    grid.TilesRoot.localRotation = Quaternion.identity;
                }
            }

            InstantiateLayout(layout);
            grid.RebuildTiles();
            ConfigurePlayer(start);

            if (HintSimulator.TryFindHint(grid, player, false, out _, out _))
            {
                lastSeed = seed + attempt;
                UpdateSeedLabel();
                onGenerationSucceeded?.Invoke();
                return;
            }
            else
            {
                Debug.LogWarning($"[Level11Generator] Hint failed on attempt {attempt} with seed {seed + attempt}.");
                DebugLayout(layout);
            }
        }

        Debug.LogError("[Level11Generator] Failed to generate a solvable layout after multiple attempts.");
    }

    void ConfigurePlayer(Vector2Int start)
    {
        player.TeleportTo(start, true);
        player.maxMoves = baseMoveCap;
        player.forceSlideMode = forceSlide;
        player.enableSlideMode = false;
        player.resetOnFailedRun = resetOnFail;
    }

    void UpdateSeedLabel()
    {
        if (seedLabel)
        {
            seedLabel.text = $"Seed: {lastSeed}";
        }
    }

    CellType[,] BuildLayout(int seed, out Vector2Int start, out Vector2Int goal)
    {
        seededRandom = new System.Random(seed);
        var gridData = new CellType[gridSize, gridSize];
        start = new Vector2Int(seededRandom.Next(gridSize), seededRandom.Next(gridSize));
        goal = new Vector2Int(seededRandom.Next(gridSize), seededRandom.Next(gridSize));
        if (goal == start)
            goal = new Vector2Int(Mathf.Clamp(goal.x - 1, 0, gridSize - 1), goal.y);
        gridData[start.x, start.y] = CellType.Start;
        gridData[goal.x, goal.y] = CellType.Goal;

        CarveGuaranteedPath(gridData, start, goal);
        PopulateRemainingCells(gridData, start, goal);
        return gridData;
    }

    void CarveGuaranteedPath(CellType[,] gridData, Vector2Int start, Vector2Int goal)
    {
        var current = start;
        while (current != goal)
        {
            var options = new List<Vector2Int>();
            if (current.x < goal.x) options.Add(Vector2Int.right);
            if (current.x > goal.x) options.Add(Vector2Int.left);
            if (current.y < goal.y) options.Add(Vector2Int.up);
            if (current.y > goal.y) options.Add(Vector2Int.down);
            if (options.Count == 0) break;
            var choice = options[seededRandom.Next(options.Count)];
            current += choice;
            if (current == goal) break;
            gridData[current.x, current.y] = RandomPathTile();
        }
    }

    CellType RandomPathTile()
    {
        double roll = seededRandom.NextDouble();
        if (roll < 0.25f) return CellType.Ice;
        if (roll < 0.45f) return CellType.Rising;
        if (roll < 0.55f) return CellType.Sticky;
        if (roll < 0.65f) return CellType.Spring;
        return CellType.Normal;
    }

    void PopulateRemainingCells(CellType[,] gridData, Vector2Int start, Vector2Int goal)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (gridData[x, y] != CellType.Normal) continue;
                var pos = new Vector2Int(x, y);
                if (pos == start || pos == goal) continue;

                double roll = seededRandom.NextDouble();
                if (roll < wallChance) gridData[x, y] = CellType.Wall;
                else if (roll < wallChance + hazardChance) gridData[x, y] = seededRandom.NextDouble() < 0.5 ? CellType.HazardFire : CellType.HazardGlass;
                else if (roll < wallChance + hazardChance + iceChance) gridData[x, y] = CellType.Ice;
                else if (roll < wallChance + hazardChance + iceChance + risingChance) gridData[x, y] = CellType.Rising;
                else if (roll < wallChance + hazardChance + iceChance + risingChance + stickyChance) gridData[x, y] = CellType.Sticky;
                else if (roll < wallChance + hazardChance + iceChance + risingChance + stickyChance + springChance) gridData[x, y] = CellType.Spring;
                else gridData[x, y] = CellType.Normal;
            }
        }
    }

    void InstantiateLayout(CellType[,] gridData)
    {
        if (!grid.TilesRoot)
        {
            Debug.LogError("[Level11Generator] Grid tiles root missing.");
            return;
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var type = gridData[x, y];
                var prefab = GetPrefabFor(type);
                if (!prefab) prefab = normalPrefab;

                var worldPos = grid.WorldFromGrid(new Vector2Int(x, y), 0f);
                var tile = Instantiate(prefab, worldPos, Quaternion.identity, grid.TilesRoot);
                tile.name = $"{type}_({x},{y})";
                spawnedTiles.Add(tile.gameObject);
            }
        }
    }

    Tile GetPrefabFor(CellType type)
    {
        switch (type)
        {
            case CellType.Goal: return goalPrefab;
            case CellType.Wall: return wallPrefab ? wallPrefab : normalPrefab;
            case CellType.Ice: return icePrefab ? icePrefab : normalPrefab;
            case CellType.Rising: return risingPrefab ? risingPrefab : normalPrefab;
            case CellType.Sticky: return stickyPrefab ? stickyPrefab : normalPrefab;
            case CellType.HazardFire: return hazardFirePrefab ? hazardFirePrefab : normalPrefab;
            case CellType.HazardGlass: return hazardGlassPrefab ? hazardGlassPrefab : normalPrefab;
            case CellType.Spring: return springPrefab ? springPrefab : normalPrefab;
            default: return normalPrefab;
        }
    }

    void ClearTiles()
    {
        foreach (var go in spawnedTiles)
        {
            if (go)
            {
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
        }
        spawnedTiles.Clear();
    }

    void DebugLayout(CellType[,] gridData)
    {
        var lines = new List<string>();
        for (int y = gridSize - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < gridSize; x++)
            {
                row += gridData[x, y].ToString().PadRight(12);
            }
            lines.Add(row);
        }
        Debug.Log("[Level11Generator] Layout:\n" + string.Join("\n", lines));
    }
}
