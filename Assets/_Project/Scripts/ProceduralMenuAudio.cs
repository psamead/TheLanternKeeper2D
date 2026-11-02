using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class ProceduralMenuAudio : MonoBehaviour
{
    private const int SampleRate = 44100;
    private AudioSource audioSource;
    private AudioClip hoverClip;
    private AudioClip selectedClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.45f;

        hoverClip = CreateHoverClip();
        selectedClip = CreateSelectedClip();
    }

    private void OnDestroy()
    {
        if (hoverClip != null) Destroy(hoverClip);
        if (selectedClip != null) Destroy(selectedClip);
    }

    public void PlayHover()
    {
        if (audioSource != null && hoverClip != null) audioSource.PlayOneShot(hoverClip, 0.35f);
    }

    public void PlaySelected()
    {
        if (audioSource != null && selectedClip != null) audioSource.PlayOneShot(selectedClip, 0.55f);
    }

    private static AudioClip CreateHoverClip()
    {
        const float duration = 0.075f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float progress = time / duration;
            float frequency = Mathf.Lerp(1450f, 980f, progress);
            phase += 2f * Mathf.PI * frequency / SampleRate;
            float attack = Mathf.Clamp01(time / 0.004f);
            float decay = Mathf.Exp(-30f * time);
            samples[i] = Mathf.Sin(phase) * attack * decay * 0.28f;
        }

        return CreateClip("Menu Hover", samples);
    }

    private static AudioClip CreateSelectedClip()
    {
        const float duration = 0.19f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phaseA = 0f;
        float phaseB = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float firstFrequency = Mathf.Lerp(520f, 470f, time / duration);
            phaseA += 2f * Mathf.PI * firstFrequency / SampleRate;

            float secondTime = time - 0.045f;
            float secondTone = 0f;
            if (secondTime > 0f)
            {
                float secondFrequency = Mathf.Lerp(780f, 700f, secondTime / (duration - 0.045f));
                phaseB += 2f * Mathf.PI * secondFrequency / SampleRate;
                secondTone = Mathf.Sin(phaseB) * Mathf.Exp(-20f * secondTime) * 0.42f;
            }

            float firstTone = Mathf.Sin(phaseA) * Mathf.Exp(-17f * time) * 0.58f;
            samples[i] = (firstTone + secondTone) * 0.32f;
        }

        return CreateClip("Menu Selected", samples);
    }

    private static AudioClip CreateClip(string clipName, float[] samples)
    {
        AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
