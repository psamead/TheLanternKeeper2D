using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LanternPickupInteractable : MonoBehaviour
{
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private StoryboardPlayer storyboardA;
    [SerializeField] private LanternVisualState maelaLantern;

    private PlayerController nearbyPlayer;
    private PlayerController pickedPlayer;
    private bool pickedUp;
    private float promptAlpha;
    private float promptTargetAlpha;

    private void Awake()
    {
        if (!storyboardA) storyboardA = FindFirstObjectByType<StoryboardPlayer>();
        if (!maelaLantern) maelaLantern = FindFirstObjectByType<LanternVisualState>();
        SetPrompt(false);
    }

    private void Update()
    {
        if (prompt)
        {
            promptAlpha = Mathf.MoveTowards(promptAlpha, promptTargetAlpha, Time.deltaTime / 0.25f);
            Color color = prompt.color;
            color.a = promptAlpha;
            prompt.color = color;
            if (promptAlpha <= 0f && promptTargetAlpha <= 0f) prompt.gameObject.SetActive(false);
        }
        if (!pickedUp && nearbyPlayer != null && Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            PickUp();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null || pickedUp) return;
        nearbyPlayer = player;
        if (prompt && flowText) prompt.text = flowText.pickupPrompt;
        SetPrompt(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearbyPlayer == null || other.GetComponentInParent<PlayerController>() != nearbyPlayer) return;
        nearbyPlayer = null;
        SetPrompt(false);
    }

    private void PickUp()
    {
        if (GameState.Instance == null || storyboardA == null) return;
        pickedPlayer = nearbyPlayer;
        if (pickedPlayer == null) return;
        pickedUp = true;
        GameState.Instance.HasLantern = true;
        if (maelaLantern) maelaLantern.SetVisible(true);
        SetPrompt(false);
        pickedPlayer.enabled = false;
        gameObject.SetActive(false);

        if (AudioManager.Instance) AudioManager.Instance.SetAmbienceDucked(true);
        storyboardA.layerA.transform.root.gameObject.SetActive(true);
        storyboardA.onFinished = FinishStoryboard;
        storyboardA.StartCoroutine(storyboardA.Play());
    }

    private void FinishStoryboard()
    {
        if (AudioManager.Instance) AudioManager.Instance.SetAmbienceDucked(false);
        if (pickedPlayer) pickedPlayer.enabled = true;
        if (storyboardA && storyboardA.layerA)
            storyboardA.layerA.transform.root.gameObject.SetActive(false);
        var hint = FindFirstObjectByType<LanternProgressHint>();
        if (hint) hint.Show();
    }

    private void SetPrompt(bool visible)
    {
        if (!prompt) return;
        promptTargetAlpha = visible ? 1f : 0f;
        if (visible) prompt.gameObject.SetActive(true);
    }
}
