using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>Core-persistent E switch, bell gating, and cold/warm Global Light blend.</summary>
public sealed class LanternSwitch : MonoBehaviour
{
    public static LanternSwitch Instance { get; private set; }
    public Light2D globalLight;
    public PlayerController player;
    public MaelaAudio maelaAudio;
    public Color presentColor = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    public float presentIntensity = 0.8f;
    public Color memoryColor = new Color32(0xF2, 0xC4, 0x8A, 0xFF);
    public float memoryIntensity = 0.92f;
    public float lightBlendSeconds = 0.6f;
    private bool bellPlaying;
    private bool hasSwitchedOnce;
    private Coroutine lightBlend;
    public bool IsBellPlaying => bellPlaying;
    public bool HasSwitchedOnce => hasSwitchedOnce;

    private void Awake() { Instance = this; }
    private void Start()
    {
        if (!player) player = FindFirstObjectByType<PlayerController>();
        if (!maelaAudio && player) maelaAudio = player.GetComponent<MaelaAudio>();
        if (GameState.Instance != null) ApplyLight(GameState.Instance.MemoryLayer, false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) RequestSwitch();
    }

    /// <summary>Shared entry point for E and deterministic in-Editor interaction checks.</summary>
    public bool RequestSwitch()
    {
        if (bellPlaying || GameState.Instance == null || !GameState.Instance.HasLantern) return false;
        StartCoroutine(SwitchWorld());
        return true;
    }

    private IEnumerator SwitchWorld()
    {
        bellPlaying = true;
        bool first = !hasSwitchedOnce;
        hasSwitchedOnce = true;
        float bellDuration = maelaAudio ? maelaAudio.PlayLanternBell() : 0.85f;
        bool targetMemory = !GameState.Instance.MemoryLayer;
        if (first && player) player.ControlsLocked = true;
        if (!first) ChangeWorld(targetMemory);
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, bellDuration));
        if (first) ChangeWorld(targetMemory);
        if (player) player.ControlsLocked = false;
        bellPlaying = false;
    }

    private void ChangeWorld(bool isMemory)
    {
        if (!isMemory && player && player.IsClimbing) player.DropToGroundSafely();
        GameState.Instance.SetMemoryLayer(isMemory);
        ApplyLight(isMemory, true);
        if (isMemory)
            foreach (var ladder in FindObjectsByType<LadderClimbZone>(FindObjectsSortMode.None))
                ladder.TryShowFirstMemoryHint(player);
    }

    private void ApplyLight(bool memory, bool animate)
    {
        if (!globalLight) globalLight = FindFirstObjectByType<Light2D>();
        if (!globalLight) return;
        if (lightBlend != null) StopCoroutine(lightBlend);
        if (animate) lightBlend = StartCoroutine(BlendLight(memory ? memoryColor : presentColor, memory ? memoryIntensity : presentIntensity));
        else { globalLight.color = memory ? memoryColor : presentColor; globalLight.intensity = memory ? memoryIntensity : presentIntensity; }
    }

    private IEnumerator BlendLight(Color targetColor, float targetIntensity)
    {
        Color fromColor = globalLight.color;
        float fromIntensity = globalLight.intensity;
        for (float t = 0f; t < lightBlendSeconds; t += Time.unscaledDeltaTime)
        {
            float p = Mathf.Clamp01(t / lightBlendSeconds);
            globalLight.color = Color.Lerp(fromColor, targetColor, p);
            globalLight.intensity = Mathf.Lerp(fromIntensity, targetIntensity, p);
            yield return null;
        }
        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
        lightBlend = null;
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}
