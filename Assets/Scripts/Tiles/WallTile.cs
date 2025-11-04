using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Wall Tile")]
public class WallTile : Tile
{
    public override bool IsWalkable => false;

    protected override Color DefaultColor => new Color(0.3f, 0.3f, 0.35f);
}
