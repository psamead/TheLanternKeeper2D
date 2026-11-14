using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LanternPickupInteractable : MonoBehaviour
{
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private StoryboardPlayer storyboardA;
    [SerializeField] private LanternVisualState maelaLantern;
    [SerializeField] private Transform returnAfterStoryA;

    private PlayerController nearbyPlayer;
    private PlayerController pickedPlayer;
    private bool pickedUp;
    private LanternProgressHint sharedHint;

    private void Awake()
    {
        if (!storyboardA) storyboardA = FindFirstObjectByType<StoryboardPlayer>();
        if (!maelaLantern) maelaLantern = FindFirstObjectByType<LanternVisualState>();
        sharedHint = FindFirstObjectByType<LanternProgressHint>();
    }

    private void Update()
    {
        if (!pickedUp && nearbyPlayer != null && !StoryboardPlayer.AnyPlaybackActive &&
            Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            PickUp();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null || pickedUp || StoryboardPlayer.AnyPlaybackActive) return;
        nearbyPlayer = player;
        if (!sharedHint) sharedHint = FindFirstObjectByType<LanternProgressHint>();
        if (sharedHint && flowText)
            sharedHint.ShowFor(this, flowText.pickupPrompt, float.PositiveInfinity, 10,
                transform, new Vector3(0f, 2.15f, 0f));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearbyPlayer == null || other.GetComponentInParent<PlayerController>() != nearbyPlayer) return;
        nearbyPlayer = null;
    }

    private void PickUp()
    {
        if (GameState.Instance == null || storyboardA == null || StoryboardPlayer.AnyPlaybackActive) return;
        pickedPlayer = nearbyPlayer;
        if (pickedPlayer == null) return;
        pickedUp = true;
        GameState.Instance.HasLantern = true;
        if (maelaLantern) maelaLantern.SetVisible(true);
        nearbyPlayer = null;
        if (sharedHint) sharedHint.Hide();
        var trigger = GetComponent<Collider2D>();
        if (trigger) trigger.enabled = false;
        var body = pickedPlayer.GetComponent<Rigidbody2D>();
        if (body) { body.linearVelocity = Vector2.zero; body.simulated = false; }
        pickedPlayer.SetControlLock(PlayerController.ControlLock.StoryReturn, true);
        pickedPlayer.enabled = false;
        gameObject.SetActive(false);

        storyboardA.layerA.transform.root.gameObject.SetActive(true);
        storyboardA.onFinishUnderBlack = ReturnUnderBlack;
        storyboardA.onFinished = FinishStoryboard;
        storyboardA.StartCoroutine(storyboardA.Play());
    }

    private IEnumerator ReturnUnderBlack()
    {
        pickedPlayer.enabled = true;
        if (!pickedPlayer.PlaceOnGround(returnAfterStoryA))
        {
            Debug.LogError("Story A return has no grounded support; keeping black and controls locked.", this);
            while (true) yield return null;
        }
        foreach (var zone in FindObjectsByType<BrokenLadderHintZone>(FindObjectsSortMode.None)) zone.ClearOccupancy();
        var level = FindFirstObjectByType<LevelInfo>();
        if (AudioManager.Instance && level)
        {
            AudioManager.Instance.SetNarrativeMusicVolume(level.musicVolume, 1f);
            AudioManager.Instance.PlayAmbience(level.ambienceClip, level.ambienceVolume, 1f);
        }
        var camera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (camera) camera.PreviousStateIsValid = false;
        yield return new WaitForFixedUpdate();
        yield return null;
        if (!pickedPlayer.IsGrounded)
        {
            Debug.LogError("Story A return lost support before reveal; keeping black.", this);
            while (true) yield return null;
        }
        yield return storyboardA.RevealHomeUnderBlack();
    }

    private void FinishStoryboard()
    {
        if (GameState.Instance) GameState.Instance.CompleteStoryA();
        if (pickedPlayer) pickedPlayer.SetControlLock(PlayerController.ControlLock.StoryReturn, false);
        if (storyboardA && storyboardA.layerA)
            storyboardA.layerA.transform.root.gameObject.SetActive(false);
    }

    private void OnDisable() { if (sharedHint) sharedHint.HideFor(this); }
}
