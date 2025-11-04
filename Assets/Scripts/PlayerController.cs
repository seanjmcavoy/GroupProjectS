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
    Direction? slideDir = null;
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
        ClearSlide();
        ClearSticky();
        //StartCoroutine(ReloadAfter(1f));
    }

    IEnumerator ReloadAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetSlide(Direction dir)
    {
        slideDir = dir;
        enableSlideMode = true;
    }

    public void ClearSlide()
    {
        slideDir = null;
        enableSlideMode = false;
    }

    public void SetSticky(int extraCost) => stickyExtraCost = Mathf.Max(stickyExtraCost, extraCost);
    public void ClearSticky() => stickyExtraCost = 0;

    public void ResetToStart()
    {
        // maybe fix to let reset while other stuff going on?
        StopAllCoroutines();
        executing = false;
        levelComplete = false;
        ClearSlide();
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
        ClearSlide();
        ClearSticky();
        executing = true;
        outOfBoundsHit = false;

        var i = 0;
        while (!levelComplete)
        {
            bool usingSlide = enableSlideMode && slideDir.HasValue;
            Direction next;
            if (usingSlide)
            {
                next = slideDir.Value;
            }
            else
            {
                if (i >= commands.Count) break; // out of commands
                next = commands[i];

                // Sticky: consume extra commands without moving
                if (stickyExtraCost > 0)
                {
                    stickyExtraCost--;
                    i++; // consume one
                    yield return new WaitForSeconds(stepTime);
                    continue;
                }

                i++;
            }

            yield return MoveAlong(next);

            if (!lastStepSucceeded)
            {
                slideDir = null;
                break;
            }

            if (levelComplete)
            {
                break;
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
        if (!TryResolveStep(dir, out var target, out var targetTile))
        {
            lastStepSucceeded = false;
            yield break;
        }

        lastStepSucceeded = true;

        var from = transform.position;
        var to = grid.WorldFromGrid(target, baseY);
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / stepTime;
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }

        GridPos = target;

        ClearSlide();
        yield return targetTile.OnEnter(this, dir);
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
