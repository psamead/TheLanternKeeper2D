using System.Collections;
using UnityEngine;

/// <summary>Scene-local Prologue loops; playback transfers to Core's AudioManager.</summary>
public sealed class PrologueAudio : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource prologueAmbienceSource;
    [SerializeField, Min(0f)] private float fadeInSeconds = 1.5f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float prologueAmbienceVolume = 0.5f;

    private void Awake()
    {
        ConfigureLoop(musicSource);
        ConfigureLoop(prologueAmbienceSource);
    }

    private void Start()
    {
        StartCoroutine(FadeIn(musicSource, musicVolume));
        StartCoroutine(FadeIn(prologueAmbienceSource, prologueAmbienceVolume));
    }

    public void HandOffToCore() => AudioManager.CapturePrologue(musicSource, prologueAmbienceSource);

    private IEnumerator FadeIn(AudioSource source, float target)
    {
        if (source == null || source.clip == null) yield break;
        source.volume = 0f;
        source.Play();
        float elapsed = 0f;
        while (elapsed < fadeInSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, target, Mathf.Clamp01(elapsed / fadeInSeconds));
            yield return null;
        }
        source.volume = target;
    }

    private static void ConfigureLoop(AudioSource source)
    {
        if (source == null) return;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
    }
}
