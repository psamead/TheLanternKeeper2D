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
    private BoxCollider2D entry;
    private readonly Collider2D[] overlaps = new Collider2D[16];
    private SpriteRenderer ladderRenderer;

    public IEnumerator WaitForVisibleLadder()
    {
        while (ladderRenderer && ladderRenderer.color.a < 0.999f) yield return null;
    }

    private void Awake()
    {
        entry = GetComponent<BoxCollider2D>();
        entry.isTrigger = true;
        entry.enabled = false;
        ladderRenderer = GetComponent<SpriteRenderer>();
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
        bool armed = !used && GameState.Instance && GameState.Instance.TutorialExited && GameState.Instance.MemoryLayer;
        entry.enabled = armed;
        nearby = null;
        if (armed)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            int count = entry.Overlap(filter, overlaps);
            for (int i = 0; i < count; i++)
            {
                var candidate = overlaps[i].GetComponentInParent<PlayerController>();
                if (candidate) { nearby = candidate; break; }
            }
        }
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

        if (!player.PlaceOnGround(atticLanding))
        {
            Debug.LogError("Attic landing has no grounded support; keeping black and controls locked.", this);
            yield break;
        }
        atticCamera.LatchAttic();
        yield return new WaitForFixedUpdate();
        if (!player.IsGrounded)
        {
            Debug.LogError("Attic landing lost support before reveal; keeping black and controls locked.", this);
            yield break;
        }
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
