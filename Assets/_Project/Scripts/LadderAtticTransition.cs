using System.Collections;
using UnityEngine;

/// <summary>Memory ladder enters the attic through one locked black transition.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class LadderAtticTransition : MonoBehaviour
{
    [SerializeField] private Transform atticLanding;
    [SerializeField] private AtticCameraZone atticCamera;
    [SerializeField] private CanvasGroup fullScreenFade;
    [SerializeField, Min(0.01f)] private float fadeSeconds = 0.4f;
    private PlayerController nearby;
    private PlayerController transitionPlayer;
    private bool transitioning;
    private bool used;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        if (fullScreenFade)
        {
            fullScreenFade.alpha = 0f;
            fullScreenFade.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player) nearby = player;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearby && other.GetComponentInParent<PlayerController>() == nearby) nearby = null;
    }

    private void Update()
    {
        if (used || transitioning || !nearby || !atticLanding || !atticCamera || !fullScreenFade ||
            StoryboardPlayer.AnyPlaybackActive || GameState.Instance == null ||
            !GameState.Instance.HasLantern || !GameState.Instance.MemoryLayer || nearby.ControlsLocked ||
            (LanternSwitch.Instance && LanternSwitch.Instance.IsBellPlaying)) return;
        StartCoroutine(EnterAttic(nearby));
    }

    private IEnumerator EnterAttic(PlayerController player)
    {
        transitioning = true;
        transitionPlayer = player;
        player.SetControlLock(PlayerController.ControlLock.AtticTransition, true);
        var hint = FindFirstObjectByType<LanternProgressHint>();
        if (hint) hint.Hide();
        var body = player.GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        fullScreenFade.gameObject.SetActive(true);
        fullScreenFade.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        player.transform.position = atticLanding.position;
        body.position = atticLanding.position;
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        atticCamera.SetAtticFraming(true);
        for (int i = 0; i < 90 && !player.IsGrounded; i++) yield return new WaitForFixedUpdate();
        if (!player.IsGrounded) Debug.LogWarning("Attic landing did not report grounded before fade-in.", this);
        yield return null; // Allow the camera to render its new confined position under black.
        yield return Fade(1f, 0f);
        fullScreenFade.blocksRaycasts = false;
        player.SetControlLock(PlayerController.ControlLock.AtticTransition, false);
        transitionPlayer = null;
        nearby = null;
        used = true;
        transitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            fullScreenFade.alpha = Mathf.Lerp(from, to, elapsed / fadeSeconds);
            yield return null;
        }
        fullScreenFade.alpha = to;
    }

    private void OnDisable()
    {
        if (transitionPlayer) transitionPlayer.SetControlLock(PlayerController.ControlLock.AtticTransition, false);
        transitionPlayer = null;
        if (fullScreenFade)
        {
            fullScreenFade.alpha = 0f;
            fullScreenFade.blocksRaycasts = false;
        }
        transitioning = false;
    }
}
