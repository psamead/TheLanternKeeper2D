using System.Collections;
using UnityEngine;

/// <summary>TEMP finish volume until the memory attic route exists.</summary>
public sealed class EndTrigger_TEMP : MonoBehaviour
{
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private float fadeSeconds = 0.8f;
    private bool loading;

    private void Awake()
    {
        if (!fade)
        {
            var overlay = GameObject.Find("EndFade_TEMP");
            if (overlay) fade = overlay.GetComponent<CanvasGroup>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (loading || GameState.Instance == null || !GameState.Instance.HasLantern || other.GetComponentInParent<PlayerController>() == null)
            return;
        StartCoroutine(LoadEndCard());
    }

    private IEnumerator LoadEndCard()
    {
        loading = true;
        var player = FindFirstObjectByType<PlayerController>();
        if (player) player.SetControlLock(PlayerController.ControlLock.EndCard, true);
        var hint = FindFirstObjectByType<LanternProgressHint>();
        if (hint) hint.Hide();
        if (fade)
        {
            fade.gameObject.SetActive(true);
            fade.blocksRaycasts = true;
            float start = fade.alpha;
            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                fade.alpha = Mathf.Lerp(start, 1f, t / fadeSeconds);
                yield return null;
            }
            fade.alpha = 1f;
        }
        SceneLoader.Load("03_EndCard");
    }
}
