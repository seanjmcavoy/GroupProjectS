using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;

    [Header("Motion")]
    public float stepTime = 0.15f;

    [Header("Limits")]
    public int maxMoves = 3;

    [Header("Mode")]
    public bool enableSlideMode = false;

    public Vector2Int GridPos { get; private set; }
    Vector2Int startGrid;
    bool executing;
    bool levelComplete;
    bool outOfBoundsHit = false;
    bool lastStepSucceeded;

    // Tile effects
    int stickyExtraCost = 0;

    float baseY; // keep current Y

    void Start()
    {
        baseY = transform.position.y;

        // infer starting grid from position
        var lp = grid.transform.InverseTransformPoint(transform.position);
        var gx = Mathf.RoundToInt(lp.x / grid.CellSize);
        var gy = Mathf.RoundToInt(lp.z / grid.CellSize);
        GridPos = startGrid = new Vector2Int(gx, gy);

        // snap to grid center
        transform.position = grid.WorldFromGrid(GridPos, baseY);
    }

    public bool IsExecuting => executing;
    public bool LastStepSucceeded => lastStepSucceeded;

    public void CompleteLevel()
    {
        if (levelComplete) return;
        levelComplete = true;
        executing = false;
        StopAllCoroutines();
        ClearSticky();
        //StartCoroutine(ReloadAfter(1f));
    }

    IEnumerator ReloadAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetSticky(int extraCost) => stickyExtraCost = Mathf.Max(stickyExtraCost, extraCost);
    public void ClearSticky() => stickyExtraCost = 0;

    public void SetSlide(Direction dir)
    {
        enableSlideMode = true;
    }

    public void ClearSlide()
    {
        enableSlideMode = false;
    }

    public void ResetToStart()
    {
        // maybe fix to let reset while other stuff going on?
        StopAllCoroutines();
        executing = false;
        levelComplete = false;
        ClearSticky();
        GridPos = startGrid;
        transform.position = grid.WorldFromGrid(GridPos, baseY);

        foreach (var tile in FindObjectsByType<RisingTile>(FindObjectsSortMode.None))
        {
            tile.ResetTile();
        }
    }

    public bool CanAddCommand(int currentCount)
    {
        return currentCount < maxMoves;
    }

    public void RunCommands(IReadOnlyList<Direction> commands)
    {
        if (executing || levelComplete) return;
        StartCoroutine(Execute(commands));
    }

    IEnumerator Execute(IReadOnlyList<Direction> commands)
    {
        ClearSticky();
        executing = true;
        outOfBoundsHit = false;

        var i = 0;
        while (i < commands.Count && !levelComplete)
        {
            Direction dir = commands[i];
            i++;

            if (stickyExtraCost > 0)
            {
                stickyExtraCost--;
                yield return new WaitForSeconds(stepTime);
                continue;
            }

            if (enableSlideMode)
            {
                // Continue sliding until you hit a wall or a non-walkable tile.
                while (TryResolveStep(dir, out var target, out var tile) && !levelComplete)
                {
                    yield return StepTo(target);
                    GridPos = target;
                    yield return tile.OnEnter(this, dir);
                    if (!tile.IsWalkable) break;
                }
            }
            else
            {
                if (!TryResolveStep(dir, out var target, out var tile))
                {
                    break; // hit wall
                }
                yield return StepTo(target);
                GridPos = target;
                yield return tile.OnEnter(this, dir);
            }
        }

        if (!levelComplete)
        {
            ResetToStart();
        }

        executing = false;
    }

    public IEnumerator ForceStep(Direction dir)
    {
        yield return MoveAlong(dir);
    }

    IEnumerator MoveAlong(Direction dir)
    {
        if (!TryResolveStep(dir, out var target, out var tile))
        {
            lastStepSucceeded = false;
            yield break;
        }

        lastStepSucceeded = true;
        yield return StepTo(target);
        GridPos = target;
        yield return tile.OnEnter(this, dir);
    }

    IEnumerator StepTo(Vector2Int target)
    {
        var from = transform.position;
        var to = grid.WorldFromGrid(target, baseY);
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / stepTime;
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
    }

    bool TryResolveStep(Direction dir, out Vector2Int target, out Tile targetTile)
    {
        target = GridPos + dir.ToOffset();
        if (!grid.TryGetTile(target, out targetTile) || !targetTile.IsWalkable)
        {
            outOfBoundsHit = true;
            return false;
        }

        outOfBoundsHit = false;
        return true;
    }
}
