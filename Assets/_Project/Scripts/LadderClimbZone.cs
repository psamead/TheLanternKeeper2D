using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Memory ladder entry and one-shot world-specific ladder hints.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class LadderClimbZone : MonoBehaviour
{
    public bool canClimb;
    public bool showMemoryHintOnFirstVisit = true;
    public bool IsClimbable => canClimb && GameState.Instance != null && GameState.Instance.MemoryLayer;
    public TMP_Text hintText;
    public LanternProgressHint sharedHint;
    public string firstMemoryHint = "The ladder is whole here.";
    public string brokenHint = "The ladder is broken.";
    public float hintSeconds = 2.5f;
    private PlayerController nearby;
    private float hideAt;
    private bool memoryHintShown;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (!sharedHint) sharedHint = FindFirstObjectByType<LanternProgressHint>();
        HideHint();
    }
    private void Update()
    {
        if (hintText && hintText.gameObject.activeSelf && Time.time >= hideAt) HideHint();
        if (nearby == null || Keyboard.current == null) return;
        bool climbingInput = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ||
                             Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        bool brokenInput = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
        if ((IsClimbable && climbingInput) || (!IsClimbable && brokenInput)) AttemptInteraction();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        nearby = player;
        if (IsClimbable) TryShowFirstMemoryHint(player);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearby && other.GetComponentInParent<PlayerController>() == nearby)
        {
            nearby.LeaveLadder(this);
            nearby = null;
        }
    }

    public void TryShowFirstMemoryHint(PlayerController player)
    {
        if (!showMemoryHintOnFirstVisit || memoryHintShown || !IsClimbable || !player) return;
        var zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider && Vector2.Distance(player.transform.position, zoneCollider.ClosestPoint(player.transform.position)) > 1f) return;
        memoryHintShown = true;
        ShowHint(firstMemoryHint, 3f);
    }

    public void AttemptInteraction()
    {
        if (nearby == null) return;
        if (IsClimbable)
        {
            TryShowFirstMemoryHint(nearby);
            nearby.EnterLadder(this);
        }
        else ShowHint(brokenHint, hintSeconds);
    }

    private void ShowHint(string text, float duration)
    {
        if (sharedHint)
        {
            sharedHint.ShowMessage(text, duration);
            return;
        }
        if (hintText)
        {
            hintText.text = text;
            hintText.gameObject.SetActive(true);
            hintText.ForceMeshUpdate();
            hideAt = Time.time + duration;
        }
    }
    private void HideHint() { if (hintText) hintText.gameObject.SetActive(false); }
}
