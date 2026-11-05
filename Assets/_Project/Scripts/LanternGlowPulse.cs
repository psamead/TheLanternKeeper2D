using UnityEngine;
using UnityEngine.UI;

/// Lantern "breathing" glow: slow sine for brightness and size, plus a small random flicker.
[RequireComponent(typeof(Graphic))]
public sealed class LanternGlowPulse : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float period = 3.2f;          // seconds per breath
    [SerializeField] private Vector2 alphaRange = new Vector2(0.30f, 1.0f);
    [SerializeField] private Vector2 scaleRange = new Vector2(0.86f, 1.10f);
    [SerializeField, Range(0f, 0.3f)] private float flicker = 0.10f;  // extra random brightness
    [SerializeField, Min(0.1f)] private float flickerSpeed = 6f;

    private Graphic graphic;
    private Vector3 baseScale;
    private float seed;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float t = Time.unscaledTime;
        float breath = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * t / period);
        float noise = (Mathf.PerlinNoise(seed, t * flickerSpeed) - 0.5f) * 2f * flicker;

        Color c = graphic.color;
        c.a = Mathf.Clamp01(Mathf.Lerp(alphaRange.x, alphaRange.y, breath) + noise);
        graphic.color = c;
        transform.localScale = baseScale * Mathf.Lerp(scaleRange.x, scaleRange.y, breath);
    }
}
