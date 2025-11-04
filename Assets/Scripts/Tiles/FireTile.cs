using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Fire Tile")]
public class FireTile : Tile
{
    protected override Color DefaultColor => new Color(0.95f, 0.25f, 0.1f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);
        Debug.Log("BUrn!!!!");
        player.ResetToStart();
    }
}
