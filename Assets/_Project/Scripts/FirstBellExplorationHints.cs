using TMPro;
using UnityEngine;

/// <summary>Two scene-anchored prompts, restored from session progress after story/fade locks.</summary>
public sealed class FirstBellExplorationHints : MonoBehaviour
{
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private TMP_Text brokenLadder;
    [SerializeField] private TMP_Text pressE;
    private PlayerController player;

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
        var state = GameState.Instance;
        bool operable = state && player && player.isActiveAndEnabled && !player.ControlsLocked &&
            !player.LanternLocked && !StoryboardPlayer.AnyPlaybackActive && !state.MemoryLayer &&
            state.ExplorationStage != GameState.FirstBellStage.Used;
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
