using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LanternPickupInteractable : MonoBehaviour
{
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private StoryboardPlayer storyboardA;
    [SerializeField] private LanternVisualState maelaLantern;

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
        pickedPlayer.enabled = false;
        gameObject.SetActive(false);

        if (AudioManager.Instance)
        {
            AudioManager.Instance.SetAmbienceDucked(true);
            AudioManager.Instance.SetMusicDucked(true);
        }
        storyboardA.layerA.transform.root.gameObject.SetActive(true);
        storyboardA.onFinished = FinishStoryboard;
        storyboardA.StartCoroutine(storyboardA.Play());
    }

    private void FinishStoryboard()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.SetAmbienceDucked(false);
            AudioManager.Instance.SetMusicDucked(false);
        }
        if (pickedPlayer)
        {
            var body = pickedPlayer.GetComponent<Rigidbody2D>();
            if (body) body.simulated = true;
            pickedPlayer.enabled = true;
        }
        if (storyboardA && storyboardA.layerA)
            storyboardA.layerA.transform.root.gameObject.SetActive(false);
        var hint = sharedHint ? sharedHint : FindFirstObjectByType<LanternProgressHint>();
        if (hint) hint.Show();
    }

    private void OnDisable() { if (sharedHint) sharedHint.HideFor(this); }
}
