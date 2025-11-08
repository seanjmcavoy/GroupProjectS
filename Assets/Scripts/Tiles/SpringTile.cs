using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Spring Tile")]
public class SpringTile : Tile
{
    [Header("Spring Settings")]
    [SerializeField, Min(1)] int launchDistance = 2;
    [SerializeField, Tooltip("If false the player bounces back the way they came in.")] bool launchForward = true;

    public int LaunchDistance => launchDistance;
    public bool LaunchForward => launchForward;

    protected override Color DefaultColor => new Color(0.3f, 0.9f, 0.45f);

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);

        var launchDir = launchForward ? fromDir : fromDir.Opposite();
        for (int step = 0; step < launchDistance; step++)
        {
            yield return player.ForceStep(launchDir);
            if (!player.LastStepSucceeded)
            {
                break;
            }
        }
    }
}
