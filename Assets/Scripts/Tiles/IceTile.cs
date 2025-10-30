using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Ice Tile")]
public class IceTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        player.SetSlide(fromDir);
        Debug.Log("SLideeee");
        yield break;
    }
}
