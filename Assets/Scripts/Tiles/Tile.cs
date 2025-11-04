using System.Collections;
using UnityEngine;

[AddComponentMenu("PathQueue/Tiles/Tile (Normal)")]
public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }
    public GridManager Grid { get; set; }

    [Header("Tile Feedback")]
    [SerializeField] ParticleSystem enterEffect;
    [SerializeField] AudioClip enterSfx;
    [Range(0f, 5f)]
    [SerializeField] float enterSfxVolume = 2.5f;
    [SerializeField] bool sfxAs2D = true;
    [Range(0.1f, 3f)]
    [SerializeField] float sfxPitch = 1f;

    [Header("Tile Tint")]
    [Tooltip("Renderers that should be tinted; leave empty to auto-find")]
    [SerializeField] Renderer[] colorTargets;
    [Tooltip("Override the default tile colour set by the tile type")]
    [SerializeField] bool overrideColorInInspector = false;
    [SerializeField] Color inspectorColor = Color.white;

    MaterialPropertyBlock colorBlock;
    static readonly int ColorProp = Shader.PropertyToID("_Color");
    static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

    public virtual bool IsWalkable => true;

    protected virtual Color DefaultColor => new Color(0.55f, 0.55f, 0.58f);

    Color EffectiveColor => overrideColorInInspector ? inspectorColor : DefaultColor;

    void OnEnable()
    {
        EnsureColorTargets();
        ApplyTileColor();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        EnsureColorTargets();
        ApplyTileColor();
    }
#endif

    protected void RefreshTileColor()
    {
        ApplyTileColor();
    }

    void EnsureColorTargets()
    {
        if (colorTargets != null && colorTargets.Length > 0) return;
        colorTargets = GetComponentsInChildren<Renderer>();
    }

    void ApplyTileColor()
    {
        if (colorTargets == null || colorTargets.Length == 0) return;
        if (colorBlock == null) colorBlock = new MaterialPropertyBlock();

        var color = EffectiveColor;
        foreach (var target in colorTargets)
        {
            if (!target) continue;
            target.GetPropertyBlock(colorBlock);
            colorBlock.SetColor(ColorProp, color);
            colorBlock.SetColor(BaseColorProp, color);
            target.SetPropertyBlock(colorBlock);
        }
    }

    public virtual IEnumerator OnEnter(PlayerController player, Direction fromDir)
    {
        PlayEnterFeedback();
        yield break;
    }

    protected void PlayEnterFeedback()
    {
        if (enterEffect)
        {
            enterEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enterEffect.Play();
        }

        if (!enterSfx) return;

        var temp = new GameObject("TileSfx (OneShot)");
        temp.transform.position = transform.position;
        var src = temp.AddComponent<AudioSource>();
        src.clip = enterSfx;
        src.volume = enterSfxVolume;
        src.pitch = sfxPitch;
        src.spatialBlend = sfxAs2D ? 0f : 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1f;
        src.maxDistance = 25f;
        src.playOnAwake = false;
        src.Play();

        float clipDuration = enterSfx.length / Mathf.Max(0.1f, Mathf.Abs(src.pitch));
        Destroy(temp, Mathf.Max(clipDuration, 0.5f));
    }
}
