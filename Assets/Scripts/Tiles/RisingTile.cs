using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Rising Tile")]
public class RisingTile : Tile
{
    [Header("Rising Settings")]
    public float riseHeight = 3f;
    public float riseSpeed = 2f;
    public bool resetOnPlayerReset = true;

    bool hasRisen = false;
    Vector3 initialPos;
    Vector3 initialScale;

    void Awake()
    {
        // Record the initial state
        initialPos = transform.position;
        initialScale = transform.localScale;
    }

    //Become an obstacle after rising.
    public override bool IsWalkable => !hasRisen;

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        if (!hasRisen)
        {
            yield return StartCoroutine(Rise());
        }
    }

    IEnumerator Rise()
    {
        hasRisen = true;

        // Move up by riseHeight from current pos
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * riseHeight;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * riseSpeed;
            transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t));
            yield return null;
        }

        transform.position = end;
         transform.localScale = new Vector3(4.5f, 10f, 4.5f);
    }

    public void ResetTile()
    {
        if (!resetOnPlayerReset) return;

        hasRisen = false;
        // Restore position and zoom
        transform.position = initialPos;
        transform.localScale = initialScale;

    }
}
