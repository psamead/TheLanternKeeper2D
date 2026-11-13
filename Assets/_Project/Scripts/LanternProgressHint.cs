using TMPro;
using UnityEngine;

public sealed class LanternProgressHint : MonoBehaviour
{
    [SerializeField] private TMP_Text hint;
    [SerializeField] private Transform followTarget;
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.55f, 0f);
    [SerializeField] private float visibleSeconds = 4f;
    private float hideAt;
    private bool visible;
    private MonoBehaviour owner;
    private int priority;
    private Transform activeAnchor;
    private Vector3 activeOffset;

    private void Awake() => Hide();

    public void Show()
    {
        if (!flowText) return;
        ShowFor(this, flowText.raiseHint, visibleSeconds, 0);
    }

    public void ShowMessage(string message, float seconds)
        => ShowFor(this, message, seconds, 0);

    public bool ShowFor(MonoBehaviour requester, string message, float seconds, int requestPriority)
        => ShowFor(requester, message, seconds, requestPriority, null, worldOffset);

    public bool ShowFor(MonoBehaviour requester, string message, float seconds, int requestPriority,
        Transform anchor, Vector3 offset)
    {
        if (!hint || StoryboardPlayer.AnyPlaybackActive || (visible && owner && owner != requester && priority > requestPriority))
            return false;
        owner = requester;
        priority = requestPriority;
        activeAnchor = anchor;
        activeOffset = offset;
        hint.text = message;
        hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, 1f);
        visible = true;
        hideAt = float.IsPositiveInfinity(seconds) ? float.PositiveInfinity : Time.time + Mathf.Max(0.1f, seconds);
        hint.gameObject.SetActive(true);
        return true;
    }

    private void Update()
    {
        if (!visible) return;
        if (StoryboardPlayer.AnyPlaybackActive) { Hide(); return; }
        Transform target = activeAnchor ? activeAnchor : followTarget;
        if (target) transform.position = target.position + activeOffset;
        float remaining = hideAt - Time.time;
        if (remaining <= 0f) { Hide(); return; }
        if (remaining < 0.6f)
            hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, Mathf.Clamp01(remaining / 0.6f));
    }

    public void Hide()
    {
        visible = false;
        owner = null;
        priority = 0;
        activeAnchor = null;
        if (hint) hint.gameObject.SetActive(false);
    }

    public void HideFor(MonoBehaviour requester)
    {
        if (owner == requester) Hide();
    }

    private void OnDisable() => Hide();
}
