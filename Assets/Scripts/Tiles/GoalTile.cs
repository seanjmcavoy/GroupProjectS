using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Goal Tile")]
public class GoalTile : Tile
{
    protected override Color DefaultColor => new Color(0.3f, 0.85f, 0.35f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);
        player.CompleteLevel();
        WinUI.Instance?.Show("You Won!");
    }
}
