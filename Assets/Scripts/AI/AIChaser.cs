using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIChaser : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] PlayerController player;
    [SerializeField] float stepTime = 0.2f;
    [SerializeField] float repathInterval = 0.5f;
    [SerializeField] float yOffset = 0f;
    [SerializeField] bool autoStart = true;

    Vector2Int gridPos;
    float baseY;
    Coroutine pathRoutine;
    Coroutine moveRoutine;
    PathOptions chaserOptions;

    void Awake()
    {
        if (grid == null)
        {
#if UNITY_2023_1_OR_NEWER
            grid = FindFirstObjectByType<GridManager>();
#else
            grid = FindObjectOfType<GridManager>();
#endif
        }

        if (player == null)
        {
#if UNITY_2023_1_OR_NEWER
            player = FindFirstObjectByType<PlayerController>();
#else
            player = FindObjectOfType<PlayerController>();
#endif
        }
        chaserOptions = PathOptions.CreateChaserDefaults();
    }

    void OnEnable()
    {
        CacheGridPosition();
        if (autoStart)
        {
            StartChasing();
        }
    }

    void OnDisable()
    {
        if (pathRoutine != null) StopCoroutine(pathRoutine);
        if (moveRoutine != null) StopCoroutine(moveRoutine);
    }

    void CacheGridPosition()
    {
        if (grid == null) return;
        baseY = transform.position.y;
        var lp = grid.transform.InverseTransformPoint(transform.position);
        gridPos = new Vector2Int(Mathf.RoundToInt(lp.x / grid.CellSize), Mathf.RoundToInt(lp.z / grid.CellSize));
        transform.position = grid.WorldFromGrid(gridPos, baseY + yOffset);
    }

    public void StartChasing()
    {
        if (pathRoutine != null) StopCoroutine(pathRoutine);
        pathRoutine = StartCoroutine(PathLoop());
    }

    IEnumerator PathLoop()
    {
        while (enabled && grid != null && player != null)
        {
            if (GridPathfinder.TryFindPath(grid, gridPos, player.GridPos, chaserOptions, out var path, out _))
            {
                if (moveRoutine != null) StopCoroutine(moveRoutine);
                moveRoutine = StartCoroutine(FollowPath(path));
            }

            yield return new WaitForSeconds(repathInterval);
        }
    }

    IEnumerator FollowPath(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0) yield break;

        for (int i = 1; i < path.Count; i++)
        {
            var target = path[i];

            if (!grid.TryGetTile(target, out var tile) || !tile.IsWalkable)
            {
                yield break;
            }

            yield return StepTo(target);
            gridPos = target;

            if (player != null && gridPos == player.GridPos)
            {
                player.ResetToStart();
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                break;
            }
        }
    }

    IEnumerator StepTo(Vector2Int target)
    {
        var from = transform.position;
        var to = grid.WorldFromGrid(target, baseY + yOffset);
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, stepTime);
            transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
    }
}
