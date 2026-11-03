using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MenuAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] hoverClips;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip backClip;

    private AudioSource source;
    private int previousHover = -1;
    private float confirmEndsAt;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.pitch = 1f;
    }

    public void PlayHover()
    {
        if (source == null || hoverClips == null || hoverClips.Length == 0) return;
        int index = Random.Range(0, hoverClips.Length);
        if (hoverClips.Length > 1 && index == previousHover)
            index = (index + Random.Range(1, hoverClips.Length)) % hoverClips.Length;
        previousHover = index;
        AudioClip clip = hoverClips[index];
        if (clip == null) return;
        source.pitch = Random.Range(0.97f, 1.03f);
        source.PlayOneShot(clip, 0.6f);
        source.pitch = 1f;
    }

    public void PlayConfirm()
    {
        if (source != null && confirmClip != null)
        {
            confirmEndsAt = Time.unscaledTime + confirmClip.length;
            source.PlayOneShot(confirmClip, 0.8f);
        }
    }

    public float ConfirmSecondsRemaining => Mathf.Max(0f, confirmEndsAt - Time.unscaledTime);

    public void PlayBack()
    {
        if (source != null && backClip != null) source.PlayOneShot(backClip, 0.7f);
    }
}
