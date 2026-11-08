using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the Prologue loops and crossfades them into the Home ambience.</summary>
public sealed class PrologueAudio : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource prologueAmbienceSource;
    [SerializeField] private AudioSource homeAmbienceSource;
    [SerializeField, Min(0f)] private float fadeInSeconds = 1.5f;
    [SerializeField, Min(0f)] private float homeCrossfadeSeconds = 2f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float prologueAmbienceVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float homeAmbienceVolume = 0.5f;

    private static PrologueAudio instance;
    private Coroutine transition;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureLoop(musicSource);
        ConfigureLoop(prologueAmbienceSource);
        ConfigureLoop(homeAmbienceSource);
    }

    private void Start()
    {
        StartCoroutine(FadeIn(musicSource, musicVolume, fadeInSeconds));
        StartCoroutine(FadeIn(prologueAmbienceSource, prologueAmbienceVolume, fadeInSeconds));
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "01_Home") return;
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(CrossfadeToHome());
    }

    private IEnumerator FadeIn(AudioSource source, float target, float seconds)
    {
        if (source == null || source.clip == null) yield break;
        source.volume = 0f;
        source.Play();
        yield return Fade(source, 0f, target, seconds);
    }

    private IEnumerator CrossfadeToHome()
    {
        float duration = Mathf.Max(0.01f, homeCrossfadeSeconds);
        float musicStart = musicSource != null ? musicSource.volume : 0f;
        float ambienceStart = prologueAmbienceSource != null ? prologueAmbienceSource.volume : 0f;

        if (homeAmbienceSource != null && homeAmbienceSource.clip != null)
        {
            homeAmbienceSource.volume = 0f;
            homeAmbienceSource.Play();
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (musicSource != null) musicSource.volume = Mathf.Lerp(musicStart, 0f, t);
            if (prologueAmbienceSource != null) prologueAmbienceSource.volume = Mathf.Lerp(ambienceStart, 0f, t);
            if (homeAmbienceSource != null) homeAmbienceSource.volume = Mathf.Lerp(0f, homeAmbienceVolume, t);
            yield return null;
        }

        if (musicSource != null) { musicSource.volume = 0f; musicSource.Stop(); }
        if (prologueAmbienceSource != null) { prologueAmbienceSource.volume = 0f; prologueAmbienceSource.Stop(); }
        if (homeAmbienceSource != null) homeAmbienceSource.volume = homeAmbienceVolume;
        transition = null;
    }

    private static IEnumerator Fade(AudioSource source, float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds));
            yield return null;
        }
        source.volume = to;
    }

    private static void ConfigureLoop(AudioSource source)
    {
        if (source == null) return;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
    }
}
