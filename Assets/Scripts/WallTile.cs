using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Wall Tile")]
public class WallTile : Tile
{
    public override bool IsWalkable => false;
}
