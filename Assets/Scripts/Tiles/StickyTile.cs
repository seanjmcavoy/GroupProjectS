using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Sticky Tile")]
public class StickyTile : Tile
{
    [SerializeField, Tooltip("Extra commands consumed before moving off this tile")]
    int extraCommandCost = 1;

    protected override Color DefaultColor => new Color(0.88f, 0.58f, 0.18f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);
        Debug.Log("sticky");
        player.SetSticky(extraCommandCost);
    }
}
