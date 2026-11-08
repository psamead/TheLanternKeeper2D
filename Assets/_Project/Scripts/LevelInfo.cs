using UnityEngine;

/// <summary>Level-owned references and presentation, applied when Core attaches.</summary>
public sealed class LevelInfo : MonoBehaviour
{
    public PolygonCollider2D cameraBounds;
    [Min(0.01f)] public float orthoSize = 3.45f;
    public AudioClip ambienceClip;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Tooltip("If musicClip is null, keep current music instead of fading it out.")]
    public bool keepMusicWhenNull;
    [Min(0f)] public float audioFadeSeconds = 2f;
    public Color globalLightColor = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    [Min(0f)] public float globalLightIntensity = 0.8f;
}
