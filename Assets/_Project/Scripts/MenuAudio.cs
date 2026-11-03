using UnityEngine;

/// Menu SFX. Hover uses two dedicated voices that alternate: a new hover starts
/// immediately on the free voice while the previous one is cut in ~15 ms, so the
/// sound follows the player's speed. If the player stops, the wave plays out fully.
/// Confirm / back use the main AudioSource (PlayOneShot) and are never cut by hover.
[RequireComponent(typeof(AudioSource))]
public sealed class MenuAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] hoverClips;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip backClip;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.6f;
    [SerializeField, Min(0.005f)] private float hoverFadeOut = 0.015f; // near-instant cut, just enough to avoid a click

    [Header("Dwell: mouse resting on one item")]
    [SerializeField, Min(0f)] private float dwellDelay = 1.0f;          // how long counts as "resting"
    [SerializeField] private Vector2 dwellInterval = new Vector2(1.4f, 2.2f); // gap between waves while resting
    [SerializeField, Range(0f, 1f)] private float dwellVolume = 0.42f;

    private AudioSource source;
    private readonly AudioSource[] hoverVoices = new AudioSource[2];
    private int activeVoice;
    private int previousHover = -1;
    private float confirmEndsAt;
    private bool pointerDwell;
    private float lastHoverTime;
    private float nextDwellWaveTime = float.MaxValue;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.pitch = 1f;

        for (int i = 0; i < hoverVoices.Length; i++)
        {
            AudioSource v = gameObject.AddComponent<AudioSource>();
            v.playOnAwake = false;
            v.loop = false;
            v.spatialBlend = 0f;
            v.outputAudioMixerGroup = source.outputAudioMixerGroup;
            hoverVoices[i] = v;
        }
    }

    /// True while the mouse rests on the selected item; false on exit / keyboard / confirm.
    public void SetPointerDwell(bool on)
    {
        pointerDwell = on;
        nextDwellWaveTime = on ? Time.unscaledTime + dwellDelay : float.MaxValue;
    }

    private void Update()
    {
        // Resting on an item long enough -> soft waves keep lapping at random intervals.
        if (pointerDwell && Time.unscaledTime >= nextDwellWaveTime)
        {
            StartHoverWave(dwellVolume);
            nextDwellWaveTime = Time.unscaledTime + Random.Range(dwellInterval.x, dwellInterval.y);
        }

        // Fade out every hover voice that is not the active one.
        float step = hoverVolume / hoverFadeOut * Time.unscaledDeltaTime;
        for (int i = 0; i < hoverVoices.Length; i++)
        {
            AudioSource v = hoverVoices[i];
            if (i == activeVoice || !v.isPlaying) continue;
            v.volume = Mathf.MoveTowards(v.volume, 0f, step);
            if (v.volume <= 0f) v.Stop();
        }
    }

    public void PlayHover()
    {
        lastHoverTime = Time.unscaledTime;
        if (pointerDwell) nextDwellWaveTime = lastHoverTime + dwellDelay; // restart the "resting" timer
        StartHoverWave(hoverVolume);
    }

    private void StartHoverWave(float volume)
    {
        if (hoverClips == null || hoverClips.Length == 0) return;
        int index = Random.Range(0, hoverClips.Length);
        if (hoverClips.Length > 1 && index == previousHover)
            index = (index + Random.Range(1, hoverClips.Length)) % hoverClips.Length;
        previousHover = index;
        AudioClip clip = hoverClips[index];
        if (clip == null) return;

        // Switch to the other voice; the old one fades out in Update().
        activeVoice = (activeVoice + 1 + hoverVoices.Length) % hoverVoices.Length;
        AudioSource v = hoverVoices[activeVoice];
        v.Stop();
        v.clip = clip;
        v.volume = volume;
        v.pitch = Random.Range(0.97f, 1.03f);
        v.Play();
    }

    /// Fade out any hover sound (used before confirm so the lantern ting is clean).
    private void ReleaseHover()
    {
        SetPointerDwell(false);
        activeVoice = -1; // no active voice: Update() fades all of them out
    }

    public void PlayConfirm()
    {
        ReleaseHover();
        if (confirmClip == null) return;
        confirmEndsAt = Time.unscaledTime + confirmClip.length;
        // Play on a temporary object that survives the scene change, so the lantern
        // "ting" is not cut when Play loads 01_Home after the 0.5 s fade.
        var go = new GameObject("ConfirmSfx");
        DontDestroyOnLoad(go);
        var oneShot = go.AddComponent<AudioSource>();
        oneShot.spatialBlend = 0f;
        oneShot.outputAudioMixerGroup = source != null ? source.outputAudioMixerGroup : null;
        oneShot.PlayOneShot(confirmClip, 0.8f);
        Destroy(go, confirmClip.length + 0.1f);
    }

    public float ConfirmSecondsRemaining => Mathf.Max(0f, confirmEndsAt - Time.unscaledTime);

    public void PlayBack()
    {
        ReleaseHover();
        if (source != null && backClip != null) source.PlayOneShot(backClip, 0.7f);
    }
}
