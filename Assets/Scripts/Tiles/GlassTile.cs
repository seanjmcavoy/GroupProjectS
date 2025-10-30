using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Glass Tile")]
public class GlassTile : Tile
{
    private bool broken = false;
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        Debug.Log("Glasss");
        player.ResetToStart();
        yield break;
    }
}
