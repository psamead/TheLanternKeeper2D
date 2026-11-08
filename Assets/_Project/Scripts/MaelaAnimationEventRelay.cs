using UnityEngine;

/// <summary>Forwards Animator events from Maela's visual child to audio on the Player root.</summary>
public sealed class MaelaAnimationEventRelay : MonoBehaviour
{
    private MaelaAudio audioOwner;

    private void Awake()
    {
        audioOwner = GetComponentInParent<MaelaAudio>();
    }

    public void PlayFootstep()
    {
        if (audioOwner == null) audioOwner = GetComponentInParent<MaelaAudio>();
        audioOwner?.PlayFootstep();
    }
}
