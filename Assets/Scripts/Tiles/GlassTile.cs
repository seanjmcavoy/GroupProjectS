using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Glass Tile")]
public class GlassTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        player.ResetToStart();
        Debug.Log("Glasss");
        yield break;
    }
}
