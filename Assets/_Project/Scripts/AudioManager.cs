using System.Collections;
using UnityEngine;

/// <summary>Core-owned music and ambience, with two voices per crossfade.</summary>
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource[] musicSources = new AudioSource[2];
    [SerializeField] private AudioSource[] ambienceSources = new AudioSource[2];
    private Coroutine musicFade, ambienceFade;
    private Coroutine ambienceDuckFade;
    private int musicVoice, ambienceVoice;
    private readonly float[] ambienceTargets = new float[2];
    private float ambienceMultiplier = 1f;

    // Only playback data crosses the single-scene Prologue -> Core boundary.
    // Neither PrologueAudio nor its AudioSources survive the scene unload.
    private struct Playback
    {
        public AudioClip clip;
        public float volume;
        public int sample;
        public bool playing;
        public static Playback Capture(AudioSource source) => source == null ? default : new Playback
        { clip = source.clip, volume = source.volume, sample = source.timeSamples, playing = source.isPlaying };
        public void Restore(AudioSource source)
        {
            source.clip = clip;
            source.volume = volume;
            if (clip == null || !playing) return;
            source.timeSamples = Mathf.Clamp(sample, 0, clip.samples - 1);
            source.Play();
        }
    }
    private static Playback pendingMusic, pendingAmbience;
    private static bool hasHandoff;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetHandoff() { hasHandoff = false; pendingMusic = pendingAmbience = default; }

    public static void CapturePrologue(AudioSource music, AudioSource ambience)
    {
        pendingMusic = Playback.Capture(music);
        pendingAmbience = Playback.Capture(ambience);
        hasHandoff = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Configure(musicSources);
        Configure(ambienceSources);
        if (hasHandoff)
        {
            pendingMusic.Restore(musicSources[0]);
            pendingAmbience.Restore(ambienceSources[0]);
            hasHandoff = false;
            pendingMusic = pendingAmbience = default;
        }
    }

    private static void Configure(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }
    }

    public void PlayMusic(AudioClip clip, float vol, float fade)
    {
        if (musicFade != null) StopCoroutine(musicFade);
        if (clip != null && musicSources[musicVoice].clip != clip) musicVoice = 1 - musicVoice;
        musicFade = StartCoroutine(Crossfade(musicSources, musicVoice, clip, vol, fade));
    }

    public void PlayAmbience(AudioClip clip, float vol, float fade)
    {
        if (ambienceFade != null) StopCoroutine(ambienceFade);
        if (clip != null && ambienceSources[ambienceVoice].clip != clip) ambienceVoice = 1 - ambienceVoice;
        ambienceTargets[1 - ambienceVoice] = 0f;
        ambienceTargets[ambienceVoice] = clip == null ? 0f : Mathf.Clamp01(vol);
        ambienceFade = StartCoroutine(Crossfade(ambienceSources, ambienceVoice, clip, vol * ambienceMultiplier, fade));
    }

    public void SetAmbienceDucked(bool ducked)
    {
        if (ambienceDuckFade != null) StopCoroutine(ambienceDuckFade);
        ambienceDuckFade = StartCoroutine(TweenAmbienceMultiplier(ducked ? 0.5f : 1f, 0.25f));
    }

    private IEnumerator TweenAmbienceMultiplier(float target, float seconds)
    {
        float start = ambienceMultiplier;
        for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
        {
            ambienceMultiplier = Mathf.Lerp(start, target, elapsed / seconds);
            for (int i = 0; i < ambienceSources.Length; i++)
                ambienceSources[i].volume = ambienceTargets[i] * ambienceMultiplier;
            yield return null;
        }
        ambienceMultiplier = target;
        for (int i = 0; i < ambienceSources.Length; i++)
            ambienceSources[i].volume = ambienceTargets[i] * ambienceMultiplier;
    }

    private static IEnumerator Crossfade(AudioSource[] sources, int voice, AudioClip clip, float volume, float seconds)
    {
        AudioSource incoming = sources[voice], outgoing = sources[1 - voice];
        if (clip != null && incoming.clip != clip)
        {
            incoming.Stop();
            incoming.clip = clip;
            incoming.volume = 0f;
        }
        if (clip != null && !incoming.isPlaying) incoming.Play();
        float fromIn = incoming.volume, fromOut = outgoing.volume;
        float target = clip == null ? 0f : Mathf.Clamp01(volume);
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            incoming.volume = Mathf.Lerp(fromIn, target, t);
            outgoing.volume = Mathf.Lerp(fromOut, 0f, t);
            yield return null;
        }
        incoming.volume = target;
        outgoing.volume = 0f;
        outgoing.Stop();
        if (clip == null) incoming.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
