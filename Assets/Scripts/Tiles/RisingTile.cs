using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Rising Tile")]
public class RisingTile : Tile
{
    [Header("Rising Settings")]
    [SerializeField] float riseHeight = 3f;
    [SerializeField] float riseSpeed = 2f;
    [SerializeField] float fallSpeed = 2f;
    [SerializeField] Vector3 raisedScale = new Vector3(4.5f, 10f, 4.5f);

    [Header("Cycle Settings")]
    [SerializeField] bool autoCycle = true;
    [SerializeField] float stayUpTime = 1.5f;
    [SerializeField] float stayDownTime = 1.5f;
    [SerializeField] bool resetOnPlayerReset = true;

    bool hasRisen;
    Vector3 initialPos;
    Vector3 initialScale;
    Coroutine cycleRoutine;

    protected override Color DefaultColor => new Color(1f, 0.82f, 0.2f);
    public override bool IsWalkable => !hasRisen;

    void Awake()
    {
        initialPos = transform.position;
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        if (autoCycle)
        {
            BeginCycle();
        }
    }

    void OnDisable()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }
    }

    public override IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        yield return base.OnEnter(player, fromDir);

        if (autoCycle) yield break;

        if (!hasRisen)
        {
            yield return null; // Delay by one frame, until the player completes the current movement.
            StartCoroutine(RiseRoutine());
        }
    }

    void BeginCycle()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
        }
        cycleRoutine = StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        ResetToInitialState();

        while (true)
        {
            if (stayDownTime > 0f)
            {
                yield return new WaitForSeconds(stayDownTime);
            }

            yield return RiseRoutine();

            if (stayUpTime > 0f)
            {
                yield return new WaitForSeconds(stayUpTime);
            }

            yield return LowerRoutine();
        }
    }

    IEnumerator RiseRoutine()
    {
        if (hasRisen) yield break;

        Vector3 start = transform.position;
        Vector3 end = initialPos + Vector3.up * riseHeight;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * riseSpeed;
            float lerp = Mathf.Clamp01(t);
            transform.position = Vector3.Lerp(start, end, lerp);
            transform.localScale = Vector3.Lerp(initialScale, raisedScale, lerp);
            yield return null;
        }

        transform.position = end;
        transform.localScale = raisedScale;
        hasRisen = true;
    }

    IEnumerator LowerRoutine()
    {
        if (!hasRisen) yield break;

        Vector3 start = transform.position;
        Vector3 end = initialPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fallSpeed;
            float lerp = Mathf.Clamp01(t);
            transform.position = Vector3.Lerp(start, end, lerp);
            transform.localScale = Vector3.Lerp(raisedScale, initialScale, lerp);
            yield return null;
        }

        transform.position = end;
        transform.localScale = initialScale;
        hasRisen = false;
    }

    public void ResetTile()
    {
        if (!resetOnPlayerReset) return;

        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }

        ResetToInitialState();

        if (autoCycle && isActiveAndEnabled)
        {
            BeginCycle();
        }
    }

    void ResetToInitialState()
    {
        hasRisen = false;
        transform.position = initialPos;
        transform.localScale = initialScale;
    }
}
