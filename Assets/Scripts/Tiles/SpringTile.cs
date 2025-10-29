using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Spring Tile")]
public class SpringTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        //bounce player away maybe arrow on tile?
        yield break;
    }
}
