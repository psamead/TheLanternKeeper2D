using System.Collections;
using UnityEngine;

/// <summary>Page cues own narrative gains; Core or the EndCard scene owns the audio lifetime.</summary>
public sealed class StoryboardAudio : MonoBehaviour
{
    private AudioSource ambience;
    private Coroutine fade;

    public void Apply(StoryboardData.AudioCue cue, AudioSource localMusic)
    {
        if (cue == null || !cue.enabled) return;
        if (localMusic)
        {
            if (!ambience)
            {
                ambience = gameObject.AddComponent<AudioSource>();
                ambience.playOnAwake = false;
                ambience.loop = true;
                ambience.spatialBlend = 0f;
                ambience.volume = 0f;
            }
            if (fade != null) StopCoroutine(fade);
            if (cue.musicStartVolume >= 0f) localMusic.volume = cue.musicStartVolume;
            fade = StartCoroutine(ApplyLocal(cue, localMusic));
        }
        else if (AudioManager.Instance)
        {
            if (cue.musicStartVolume >= 0f) AudioManager.Instance.SetNarrativeMusicVolume(cue.musicStartVolume, 0f);
            AudioManager.Instance.SetNarrativeMusicVolume(cue.musicVolume, cue.fadeSeconds);
            AudioManager.Instance.PlayAmbience(cue.ambience, cue.ambienceVolume, cue.fadeSeconds);
        }
    }

    private IEnumerator ApplyLocal(StoryboardData.AudioCue cue, AudioSource music)
    {
        if (cue.ambience && ambience.clip != cue.ambience)
        {
            ambience.Stop();
            ambience.clip = cue.ambience;
            ambience.volume = 0f;
        }
        if (cue.ambience && !ambience.isPlaying) ambience.Play();
        float musicFrom = music.volume, ambienceFrom = ambience.volume;
        float ambienceTo = cue.ambience ? cue.ambienceVolume : 0f;
        for (float t = 0f; t < cue.fadeSeconds; t += Time.unscaledDeltaTime)
        {
            music.volume = Mathf.Lerp(musicFrom, cue.musicVolume, t / cue.fadeSeconds);
            ambience.volume = Mathf.Lerp(ambienceFrom, ambienceTo, t / cue.fadeSeconds);
            yield return null;
        }
        music.volume = cue.musicVolume;
        ambience.volume = ambienceTo;
        if (!cue.ambience) ambience.Stop();
        fade = null;
    }

    private void OnDisable()
    {
        if (fade != null) StopCoroutine(fade);
        fade = null;
        if (ambience) ambience.Stop();
    }
}
