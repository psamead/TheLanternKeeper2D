using System.Collections;
using UnityEngine;

public enum WorldLayerMode { Present, Memory, Both }

/// <summary>Fades world-specific sprites while enabling their colliders at the switch instant.</summary>
public sealed class WorldLayerObject : MonoBehaviour
{
    public WorldLayerMode mode = WorldLayerMode.Both;
    [Min(0.01f)] public float fadeSeconds = 0.6f;
    private SpriteRenderer[] renderers;
    private Collider2D[] colliders;
    private Coroutine fade;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
        Apply(GameState.Instance != null && GameState.Instance.MemoryLayer, false);
    }

    private void OnEnable()
    {
        if (GameState.Instance != null) GameState.Instance.OnWorldSwitched += HandleSwitch;
    }

    private void OnDisable()
    {
        if (GameState.Instance != null) GameState.Instance.OnWorldSwitched -= HandleSwitch;
    }

    private void HandleSwitch(bool isMemory) => Apply(isMemory, true);

    private void Apply(bool isMemory, bool animate)
    {
        bool visible = mode == WorldLayerMode.Both ||
                       (mode == WorldLayerMode.Memory && isMemory) ||
                       (mode == WorldLayerMode.Present && !isMemory);
        foreach (var collider in colliders) if (collider) collider.enabled = visible;
        if (fade != null) StopCoroutine(fade);
        if (animate && isActiveAndEnabled) fade = StartCoroutine(Fade(visible ? 1f : 0f));
        else SetAlpha(visible ? 1f : 0f);
    }

    private IEnumerator Fade(float target)
    {
        float[] starts = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) starts[i] = renderers[i] ? renderers[i].color.a : 0f;
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(starts.Length > 0 ? starts[0] : target, target, t / fadeSeconds), starts, target, t / fadeSeconds);
            yield return null;
        }
        SetAlpha(target);
        fade = null;
    }

    private void SetAlpha(float alpha) => SetAlpha(alpha, null, alpha, 1f);
    private void SetAlpha(float unused, float[] starts, float target, float t)
    {
        if (starts == null)
        {
            for (int i = 0; i < renderers.Length; i++) SetRendererAlpha(i, target);
            return;
        }
        for (int i = 0; i < renderers.Length; i++) SetRendererAlpha(i, Mathf.Lerp(starts[i], target, t));
    }
    private void SetRendererAlpha(int i, float alpha)
    {
        if (!renderers[i]) return;
        Color c = renderers[i].color; c.a = alpha; renderers[i].color = c;
    }
}
