using TMPro;
using UnityEngine;

/// <summary>Two scene-anchored prompts, restored from session progress after story/fade locks.</summary>
public sealed class FirstBellExplorationHints : MonoBehaviour
{
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private TMP_Text brokenLadder;
    [SerializeField] private TMP_Text pressE;
    private PlayerController player;
    [SerializeField, Min(0.01f)] private float exitFadeSeconds = 0.5f;
    private bool exiting;

    public System.Collections.IEnumerator ExitTutorial()
    {
        exiting = true;
        foreach (var zone in FindObjectsByType<BrokenLadderHintZone>(FindObjectsSortMode.None)) zone.Retire();
        for (float t = 0; t < exitFadeSeconds; t += Time.unscaledDeltaTime)
        {
            if (brokenLadder) brokenLadder.alpha = 1f - t / exitFadeSeconds;
            if (pressE) pressE.alpha = 1f - t / exitFadeSeconds;
            yield return null;
        }
        SetVisible(brokenLadder, false);
        SetVisible(pressE, false);
    }

    private void Awake()
    {
        SetVisible(brokenLadder, false);
        SetVisible(pressE, false);
        if (flowText)
        {
            if (brokenLadder) brokenLadder.text = flowText.brokenLadderHint;
            if (pressE) pressE.text = flowText.raiseHint;
        }
    }

    private void Start() => player = FindFirstObjectByType<PlayerController>();

    private void LateUpdate()
    {
        if (exiting) return;
        var state = GameState.Instance;
        bool ringing = LanternSwitch.Instance && LanternSwitch.Instance.IsBellPlaying && state && !state.TutorialExited;
        bool operable = state && state.HasLantern && state.StoryACompleted && player &&
            player.isActiveAndEnabled && !StoryboardPlayer.AnyPlaybackActive &&
            (ringing || (!player.ControlsLocked && !player.LanternLocked && !state.MemoryLayer &&
            state.ExplorationStage != GameState.FirstBellStage.Used));
        SetVisible(brokenLadder, operable && state.ExplorationStage >= GameState.FirstBellStage.Discovered);
        SetVisible(pressE, operable && state.ExplorationStage >= GameState.FirstBellStage.Inspected);
    }

    private static void SetVisible(TMP_Text text, bool visible)
    {
        if (text && text.gameObject.activeSelf != visible) text.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        SetVisible(brokenLadder, false);
        SetVisible(pressE, false);
    }
}
