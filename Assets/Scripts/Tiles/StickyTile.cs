using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Sticky Tile")]
public class StickyTile : Tile
{
    [SerializeField, Tooltip("Extra commands consumed before moving off this tile")]
    int extraCommandCost = 1;

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        Debug.Log("sticky");
        player.SetSticky(extraCommandCost);
        yield break;
    }
}
