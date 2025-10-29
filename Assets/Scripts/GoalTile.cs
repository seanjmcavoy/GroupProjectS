using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Goal Tile")]
public class GoalTile : Tile
{
    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        player.CompleteLevel();
        WinUI.Instance?.Show("You Won!");
        yield break;
    }
}
