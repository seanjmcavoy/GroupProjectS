using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Ice Tile")]
public class IceTile : Tile
{
    protected override Color DefaultColor => new Color(0.55f, 0.8f, 1f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);
        player.SetSlide(fromDir);
    }
}
