using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LanternProgressHint : MonoBehaviour
{
    [SerializeField] private TMP_Text hint;
    [SerializeField] private Transform followTarget;
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);
    [SerializeField] private float visibleSeconds = 4f;
    private float hideAt;
    private bool visible;

    private void Awake() => Hide();

    public void Show()
    {
        if (!flowText) return;
        ShowMessage(flowText.raiseHint, visibleSeconds);
    }

    public void ShowMessage(string message, float seconds)
    {
        if (!hint) return;
        hint.text = message;
        hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, 1f);
        visible = true;
        hideAt = Time.time + Mathf.Max(0.1f, seconds);
        hint.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!visible) return;
        if (followTarget) transform.position = followTarget.position + worldOffset;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) { Hide(); return; }
        float remaining = hideAt - Time.time;
        if (remaining <= 0f) { Hide(); return; }
        if (remaining < 0.6f)
            hint.color = new Color(hint.color.r, hint.color.g, hint.color.b, Mathf.Clamp01(remaining / 0.6f));
    }

    public void Hide()
    {
        visible = false;
        if (hint) hint.gameObject.SetActive(false);
    }
}
