using System.Collections.Generic;
using UnityEngine;

public static class HintSimulator
{
    class SimState
    {
        public Vector2Int Pos;
        public bool EnableSlide;
        public bool ForceSlide;
        public Direction? SlideDir;
        public int StickyCost;
        public ulong RaisedMask;

        public SimState Clone()
        {
            return new SimState
            {
                Pos = Pos,
                EnableSlide = EnableSlide,
                ForceSlide = ForceSlide,
                SlideDir = SlideDir,
                StickyCost = StickyCost,
                RaisedMask = RaisedMask
            };
        }
    }

    class Node
    {
        public SimState State;
        public List<Direction> Commands;
        public List<Vector2Int> Path;
    }

    struct StateKey
    {
        public int X;
        public int Y;
        public bool EnableSlide;
        public bool ForceSlide;
        public int SlideDirIndex;
        public int StickyCost;
        public ulong RaisedMask;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X;
                hash = (hash * 397) ^ Y;
                hash = (hash * 397) ^ (EnableSlide ? 1 : 0);
                hash = (hash * 397) ^ (ForceSlide ? 1 : 0);
                hash = (hash * 397) ^ SlideDirIndex;
                hash = (hash * 397) ^ StickyCost;
                hash = (hash * 397) ^ RaisedMask.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StateKey other)
            {
                return X == other.X &&
                       Y == other.Y &&
                       EnableSlide == other.EnableSlide &&
                       ForceSlide == other.ForceSlide &&
                       SlideDirIndex == other.SlideDirIndex &&
                       StickyCost == other.StickyCost &&
                       RaisedMask == other.RaisedMask;
            }
            return false;
        }
    }

    static Dictionary<Vector2Int, int> risingLookup = new Dictionary<Vector2Int, int>();
    static Dictionary<Vector2Int, Tile> fallbackTiles = new Dictionary<Vector2Int, Tile>();

const int DebugLimit = 200;
static bool debugMode;
static int debugCount;

    public static bool TryFindHint(GridManager grid, PlayerController player, bool debug, out List<Vector2Int> path, out List<Direction> commands)
    {
    path = null;
    commands = null;

    debugMode = debug;
    debugCount = 0;

    if (grid == null || player == null)
    {
        Debug.LogWarning("[HintSimulator] Missing grid or player.");
        return false;
    }

        var goalPositions = CollectGoalPositions();
        if (goalPositions.Count == 0)
        {
            Debug.LogWarning("[HintSimulator] No goal tiles found in scene.");
            return false;
        }

        BuildRisingLookup();
        BuildFallbackTileCache(grid);

        var startState = new SimState
        {
            Pos = player.GridPos,
            EnableSlide = player.enableSlideMode || player.forceSlideMode,
            ForceSlide = player.forceSlideMode,
            SlideDir = null,
            StickyCost = 0,
            RaisedMask = 0
        };

        var visited = new HashSet<StateKey>();
        var queue = new Queue<Node>();
        queue.Enqueue(new Node
        {
            State = startState,
            Commands = new List<Direction>(),
            Path = new List<Vector2Int> { startState.Pos }
        });
        visited.Add(KeyFromState(startState));

        int maxMoves = Mathf.Max(1, player.maxMoves);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            if (goalPositions.Contains(node.State.Pos))
            {
                path = node.Path;
                commands = node.Commands;
                return true;
            }

        bool commandLimitReached = node.Commands.Count >= maxMoves;

        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            if (SimulateCommand(grid, node.State, dir, out var nextState, out var visitTrail, out bool reachedGoal, out bool consumedCommand))
            {
                if (consumedCommand && commandLimitReached)
                    continue;

                var nextCommands = node.Commands;
                if (consumedCommand)
                {
                    nextCommands = new List<Direction>(node.Commands) { dir };
                    }

                    var nextPath = new List<Vector2Int>(node.Path);
                    nextPath.AddRange(visitTrail);

                    if (reachedGoal || goalPositions.Contains(nextState.Pos))
                    {
                        path = nextPath;
                        commands = nextCommands;
                        return true;
                    }

                    var key = KeyFromState(nextState);
                    if (!visited.Add(key)) continue;

                    queue.Enqueue(new Node
                    {
                        State = nextState,
                        Commands = nextCommands,
                        Path = nextPath
                    });
                }
            }
        }

        if (debugMode)
        {
            Debug.LogWarning($"[HintSimulator] Explored {visited.Count} states but found no path.");
        }
        Debug.LogWarning("[HintSimulator] No valid hint path found.");
        return false;
    }

    static HashSet<Vector2Int> CollectGoalPositions()
    {
#if UNITY_2023_1_OR_NEWER
        var goals = Object.FindObjectsByType<GoalTile>(FindObjectsSortMode.None);
#else
        var goals = Object.FindObjectsOfType<GoalTile>();
#endif
        var result = new HashSet<Vector2Int>();
        foreach (var goal in goals)
        {
            if (goal != null)
                result.Add(goal.GridPos);
        }
        return result;
    }

    static void BuildRisingLookup()
    {
        risingLookup.Clear();
#if UNITY_2023_1_OR_NEWER
        var risers = Object.FindObjectsByType<RisingTile>(FindObjectsSortMode.None);
#else
        var risers = Object.FindObjectsOfType<RisingTile>();
#endif
        int index = 0;
        foreach (var rising in risers)
        {
            if (rising == null) continue;
            if (!risingLookup.ContainsKey(rising.GridPos))
            {
                risingLookup[rising.GridPos] = index;
                index++;
                if (index >= 64) break;
            }
        }
    }


    static bool SimulateCommand(GridManager grid, SimState originalState, Direction inputDir, out SimState resultState, out List<Vector2Int> visitedPositions, out bool reachedGoal, out bool consumedCommand)
    {
        visitedPositions = new List<Vector2Int>();
        reachedGoal = false;
        consumedCommand = false;

        var state = originalState.Clone();

        Direction dir;
        bool directionWasSlide = state.SlideDir.HasValue;
        consumedCommand = !directionWasSlide;
        if (directionWasSlide)
        {
            dir = state.SlideDir.Value;
        }
        else
        {
            dir = inputDir;
            if (state.StickyCost > 0)
            {
                state.StickyCost--;
                resultState = state;
                return true;
            }
        }

        bool sliding = state.EnableSlide || state.ForceSlide || directionWasSlide;

        if (!sliding)
        {
            if (!TryStep(grid, state, dir, visitedPositions, out reachedGoal))
            {
                resultState = originalState.Clone();
                return false;
            }
            resultState = state;
            return true;
        }

        bool moved = false;
        bool blocked = false;

        int slideSafety = 256;
        while (slideSafety-- > 0)
        {
            if (!TryStep(grid, state, dir, visitedPositions, out reachedGoal))
            {
                blocked = true;
                break;
            }

            moved = true;

            if (reachedGoal)
            {
                state.SlideDir = null;
                state.EnableSlide = false;
                break;
            }

            if (state.SlideDir.HasValue)
            {
                dir = state.SlideDir.Value;
                continue;
            }
            else if (state.ForceSlide)
            {
                state.SlideDir = dir;
                continue;
            }
            else
            {
                state.EnableSlide = false;
                break;
            }
        }

        if (blocked)
        {
            state.SlideDir = null;
            if (!state.ForceSlide)
                state.EnableSlide = false;
        }
        else if (!state.ForceSlide && !state.SlideDir.HasValue)
        {
            state.EnableSlide = false;
        }

        resultState = state;
        return moved || (!directionWasSlide && !blocked);
    }

    static bool TryStep(GridManager grid, SimState state, Direction dir, List<Vector2Int> visited, out bool goalReached)
    {
        goalReached = false;
        var offset = dir.ToOffset();
        var previous = state.Pos;
        var target = previous + offset;

        if (!grid.TryGetTile(target, out var tile) && !fallbackTiles.TryGetValue(target, out tile))
        {
            LogDebug($"Blocked trying to move from {state.Pos} to {target} (tile=None).");
            return false;
        }

        if (!IsWalkable(state, tile))
        {
            string tileType = tile ? tile.GetType().Name : "None";
            LogDebug($"Blocked trying to move from {state.Pos} to {target} (tile={tileType}, walkable={tile?.IsWalkable ?? false}).");
            return false;
        }

        visited.Add(target);
        state.Pos = target;

        if (tile is FireTile || tile is GlassTile)
        {
            LogDebug($"Stopped on hazard tile {tile.GetType().Name} at {tile.GridPos}.");
            return false;
        }

        if (tile is RisingTile)
        {
            MarkRaised(state, tile.GridPos);
            LogDebug($"Marked rising tile at {tile.GridPos} as raised.");
        }
        else if (tile is SpringTile spring)
        {
            Direction launchDir = spring.LaunchForward ? dir : dir.Opposite();
            LogDebug($"Spring tile at {tile.GridPos} launching {launchDir} for {spring.LaunchDistance}.");

            for (int i = 0; i < spring.LaunchDistance && !goalReached; i++)
            {
                if (!TryStep(grid, state, launchDir, visited, out goalReached))
                {
                    LogDebug("Spring launch blocked before completing distance.");
                    break;
                }
            }
        }

        if (tile is StickyTile sticky)
        {
            state.StickyCost = Mathf.Max(state.StickyCost, sticky.ExtraCommandCost);
        }

        if (tile is IceTile)
        {
            state.EnableSlide = true;
            state.SlideDir = dir;
        }
        else if (!state.ForceSlide)
        {
            state.SlideDir = null;
            state.EnableSlide = false;
        }

        if (tile is GoalTile)
        {
            goalReached = true;
            LogDebug($"Reached goal at {tile.GridPos}.");
        }

        if (state.ForceSlide && !state.SlideDir.HasValue)
        {
            state.SlideDir = dir;
            state.EnableSlide = true;
        }

        LogDebug($"Stepped from {previous} to {target} on {tile.GetType().Name}.");
        return true;
    }

    static bool IsWalkable(SimState state, Tile tile)
    {
        if (tile == null)
        {
            Debug.Log("[HintSimulator] Hit out of bounds during simulation.");
            return false;
        }
        if (tile is RisingTile && IsRaised(state, tile.GridPos))
        {
            LogDebug($"Rising tile at {tile.GridPos} already raised, treating as wall.");
            return false;
        }
        return tile.IsWalkable;
    }

    static void MarkRaised(SimState state, Vector2Int pos)
    {
        if (!risingLookup.TryGetValue(pos, out var index)) return;
        if (index >= 64) return;
        state.RaisedMask |= (1UL << index);
    }

    static bool IsRaised(SimState state, Vector2Int pos)
    {
        if (!risingLookup.TryGetValue(pos, out var index)) return false;
        if (index >= 64) return false;
        return (state.RaisedMask & (1UL << index)) != 0;
    }

    static StateKey KeyFromState(SimState state)
    {
        int slideIndex = state.SlideDir.HasValue ? (int)state.SlideDir.Value + 1 : 0;
        int sticky = Mathf.Clamp(state.StickyCost, 0, 8);
        return new StateKey
        {
            X = state.Pos.x,
            Y = state.Pos.y,
            EnableSlide = state.EnableSlide,
            ForceSlide = state.ForceSlide,
            SlideDirIndex = slideIndex,
            StickyCost = sticky,
            RaisedMask = state.RaisedMask
        };
    }

    static void BuildFallbackTileCache(GridManager grid)
    {
        fallbackTiles.Clear();
        if (grid == null) return;
#if UNITY_2023_1_OR_NEWER
        var tiles = grid.GetComponentsInChildren<Tile>(true);
#else
        var tiles = grid.GetComponentsInChildren<Tile>(true);
#endif
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            var local = grid.transform.InverseTransformPoint(tile.transform.position);
            int gx = Mathf.RoundToInt(local.x / grid.CellSize);
            int gy = Mathf.RoundToInt(local.z / grid.CellSize);
            fallbackTiles[new Vector2Int(gx, gy)] = tile;
        }
    }

    static void LogDebug(string message)
    {
        if (!debugMode) return;
        if (debugCount >= DebugLimit) return;
        debugCount++;
        Debug.Log($"[HintSimulator] {message}");
    }
}
