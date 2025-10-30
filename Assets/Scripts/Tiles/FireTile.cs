using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Fire Tile")]
public class FireTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        Debug.Log("BUrn!!!!");
        player.ResetToStart();
        yield break;
    }
}
