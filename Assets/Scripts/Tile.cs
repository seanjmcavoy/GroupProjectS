using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Tile (Normal)")]
public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public GridManager Grid { get; set; }
    public virtual bool IsWalkable => true;
    public virtual IEnumerator OnEnter(PlayerController player, Direction fromDir) { yield break; }
}
