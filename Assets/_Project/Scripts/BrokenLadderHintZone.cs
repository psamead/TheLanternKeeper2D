using UnityEngine;

/// <summary>Present-world information at the empty ladder position.</summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class BrokenLadderHintZone : MonoBehaviour
{
    [SerializeField] private LanternFlowText flowText;
    private LanternProgressHint hint;
    private PlayerController nearby;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        hint = FindFirstObjectByType<LanternProgressHint>();
    }

    private void OnEnable()
    {
        if (GameState.Instance) GameState.Instance.OnWorldSwitched += OnWorldSwitched;
    }

    private void OnDisable()
    {
        if (GameState.Instance) GameState.Instance.OnWorldSwitched -= OnWorldSwitched;
        if (hint) hint.HideFor(this);
        nearby = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (!player) return;
        nearby = player;
        Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (nearby && other.GetComponentInParent<PlayerController>() == nearby)
        {
            nearby = null;
            if (hint) hint.HideFor(this);
        }
    }

    private void OnWorldSwitched(bool isMemory)
    {
        if (isMemory && hint) hint.HideFor(this);
        else if (!isMemory) Show();
    }

    private void Show()
    {
        if (!nearby || !flowText || (GameState.Instance && GameState.Instance.MemoryLayer)) return;
        if (!hint) hint = FindFirstObjectByType<LanternProgressHint>();
        if (hint) hint.ShowFor(this, flowText.brokenLadderHint, float.PositiveInfinity, 5);
    }
}
