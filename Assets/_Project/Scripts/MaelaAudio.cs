using UnityEngine;

/// <summary>One-shot Maela sounds, driven by controller events and animation events.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class MaelaAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] landClips;
    [SerializeField] private AudioClip lanternRaiseClip;

    private AudioSource source;
    private AudioSource lanternSource;
    private PlayerController controller;
    private AudioClip previousClip;
    private bool wasGrounded;
    private float airborneSeconds;

    public float LanternRaiseDuration => lanternRaiseClip != null ? lanternRaiseClip.length : 0f;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        var sources = GetComponents<AudioSource>();
        lanternSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        lanternSource.playOnAwake = false;
        lanternSource.loop = false;
        lanternSource.spatialBlend = 0f;
        lanternSource.pitch = 1f;
        lanternSource.outputAudioMixerGroup = source.outputAudioMixerGroup;
        controller = GetComponent<PlayerController>();
        wasGrounded = controller != null && controller.IsGrounded;
    }

    private void OnEnable()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (controller == null) return;
        controller.OnJump += PlayJump;
        controller.OnLanternRaised += PlayLanternRaise;
    }

    private void OnDisable()
    {
        if (controller == null) return;
        controller.OnJump -= PlayJump;
        controller.OnLanternRaised -= PlayLanternRaise;
    }

    private void Update()
    {
        if (controller == null) return;
        bool grounded = controller.IsGrounded;
        if (grounded)
        {
            if (!wasGrounded && airborneSeconds > 0.15f) PlayRandom(landClips, 0.9f);
            airborneSeconds = 0f;
        }
        else
        {
            airborneSeconds += Time.deltaTime;
        }
        wasGrounded = grounded;
    }

    // Called by the Walk and Run animation event relay on the Animator child.
    public void PlayFootstep()
    {
        if (controller != null && controller.IsGrounded)
            PlayRandom(footstepClips, controller.IsWalking ? 0.55f : 0.7f);
    }

    private void PlayJump() => PlayRandom(jumpClips, 0.8f);

    private void PlayLanternRaise()
    {
        if (lanternSource == null || lanternRaiseClip == null) return;
        lanternSource.Stop();
        lanternSource.clip = lanternRaiseClip;
        lanternSource.volume = 0.8f;
        lanternSource.pitch = 1f;
        lanternSource.Play();
    }

    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        int first = Random.Range(0, clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[(first + i) % clips.Length];
            if (clip == null || (clips.Length > 1 && clip == previousClip)) continue;
            PlayClip(clip, volume);
            return;
        }
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clip, volume);
        previousClip = clip;
    }
}
