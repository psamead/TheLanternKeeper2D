using System.Collections;
using UnityEngine;

public sealed class MenuMusic : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField, Min(0f)] private float fadeInSeconds = 2f;
    [SerializeField, Min(0f)] private float fadeOutSeconds = 1f;

    private Coroutine transition;

    private void Start()
    {
        if (musicSource != null) StartCoroutine(FadeIn(musicSource, 0.6f));
        if (ambienceSource != null) StartCoroutine(FadeIn(ambienceSource, 0.45f));
    }

    public void FadeOutAndLoad(string sceneName)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(FadeOutThenLoad(sceneName));
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume)
    {
        source.volume = 0f;
        if (!source.isPlaying) source.Play();
        float elapsed = 0f;
        while (elapsed < fadeInSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, Mathf.Clamp01(elapsed / fadeInSeconds));
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeOutThenLoad(string sceneName)
    {
        // Keep the confirm clip audible before beginning the one-second music fade.
        MenuAudio menuAudio = FindFirstObjectByType<MenuAudio>();
        if (menuAudio != null && menuAudio.ConfirmSecondsRemaining > 0f)
            yield return new WaitForSecondsRealtime(menuAudio.ConfirmSecondsRemaining);

        float musicStart = musicSource != null ? musicSource.volume : 0f;
        float ambienceStart = ambienceSource != null ? ambienceSource.volume : 0f;
        float elapsed = 0f;
        while (elapsed < fadeOutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeOutSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeOutSeconds);
            if (musicSource != null) musicSource.volume = Mathf.Lerp(musicStart, 0f, t);
            if (ambienceSource != null) ambienceSource.volume = Mathf.Lerp(ambienceStart, 0f, t);
            yield return null;
        }
        SceneLoader.Load(sceneName);
    }
}
