using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Glass Tile")]
public class GlassTile : Tile
{
    protected override Color DefaultColor => new Color(0.8f, 0.95f, 1f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);
        player.ResetToStart();
        Debug.Log("Glasss");
    }
}
