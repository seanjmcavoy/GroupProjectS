using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Fire Tile")]
public class FireTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield break;
    }
}
