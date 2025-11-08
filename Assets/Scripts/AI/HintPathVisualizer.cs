using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HintPathVisualizer : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] PlayerController player;
    [SerializeField] KeyCode hotkey = KeyCode.H;
    [SerializeField] float hintYOffset = 0.1f;
    [SerializeField] float displayDuration = 3f;
    [SerializeField] Color lineColor = Color.cyan;
    [SerializeField] float lineWidth = 0.12f;
    [Header("Debug")]
    [SerializeField] bool debugLogging = false;

    LineRenderer line;
    Coroutine hideRoutine;

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
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;
        line.positionCount = 0;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = lineColor;
    }

    void OnValidate()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (line != null)
        {
            line.widthMultiplier = lineWidth;
            line.startColor = line.endColor = lineColor;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(hotkey))
        {
            ShowHint();
        }
    }

    void ShowHint()
    {
        if (grid == null || player == null) return;

        if (!HintSimulator.TryFindHint(grid, player, debugLogging, out var path, out var _))
        {
            Debug.LogWarning("[HintPathVisualizer] No hint path available.");
            HideLine();
            return;
        }

        RenderPath(path);
    }

    void RenderPath(List<Vector2Int> path)
    {
        var positions = new Vector3[path.Count];
        float y = player != null ? player.transform.position.y + hintYOffset : hintYOffset;
        for (int i = 0; i < path.Count; i++)
        {
            positions[i] = grid.WorldFromGrid(path[i], y);
        }

        line.positionCount = positions.Length;
        line.SetPositions(positions);
        line.enabled = true;

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(displayDuration));
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideLine();
    }

    void HideLine()
    {
        if (line != null)
        {
            line.positionCount = 0;
            line.enabled = false;
        }
    }
}
