using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] float cellSize = 1f;
    [SerializeField] Transform tilesRoot; // parent of all tiles

    readonly Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
    public float CellSize => cellSize;

    void Awake()
    {
        tiles.Clear();
        foreach (var tile in tilesRoot.GetComponentsInChildren<Tile>())
        {
            tile.Grid = this;

            // infer grid from local position
            var lp = transform.InverseTransformPoint(tile.transform.position);
            var gx = Mathf.RoundToInt(lp.x / cellSize);
            var gy = Mathf.RoundToInt(lp.z / cellSize);

            tile.GridPos = new Vector2Int(gx, gy);
            tiles[tile.GridPos] = tile;
            

        }
        Debug.Log($"[Grid] Tiles found: {tilesRoot.GetComponentsInChildren<Tile>().Length}, cellSize={cellSize}");
    }
 
    public bool TryGetTile(Vector2Int gridPos, out Tile tile) => tiles.TryGetValue(gridPos, out tile);

    public Vector3 WorldFromGrid(Vector2Int g, float y)
        => transform.TransformPoint(new Vector3(g.x * cellSize, y, g.y * cellSize));
}
