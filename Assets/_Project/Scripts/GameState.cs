using UnityEngine;
using System;

/// <summary>Session state owned by the loaded Core scene.</summary>
public sealed class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    private static bool hasLantern;
    private static bool memoryLayer;
    public enum FirstBellStage { Unexplored, Discovered, Inspected, Used }
    private static FirstBellStage firstBellStage;
    private static bool storyACompleted;
    private static bool tutorialExited;
    public bool TutorialExited => tutorialExited;
    private static int explorationFrame = -1;
    public FirstBellStage ExplorationStage => firstBellStage;
    public bool StoryACompleted => storyACompleted;
    public bool CanUseLantern => hasLantern && storyACompleted &&
        firstBellStage >= FirstBellStage.Inspected && Time.frameCount > explorationFrame;
    public bool HasLantern { get => hasLantern; set => hasLantern = value; }
    public bool MemoryLayer { get => memoryLayer; set => memoryLayer = value; }
    public event Action<bool> OnWorldSwitched;

    public void SetMemoryLayer(bool value)
    {
        if (memoryLayer == value) return;
        memoryLayer = value;
        OnWorldSwitched?.Invoke(value);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState() { ResetForMenu(); Instance = null; }

    public void CompleteStoryA() => storyACompleted = true;

    public bool RecordExploration(bool discovery, PlayerController player)
    {
        if (!hasLantern || !storyACompleted || !player || !player.isActiveAndEnabled || player.ControlsLocked || player.LanternLocked ||
            StoryboardPlayer.AnyPlaybackActive || memoryLayer || Time.frameCount == explorationFrame) return false;
        if (discovery ? firstBellStage != FirstBellStage.Unexplored : firstBellStage != FirstBellStage.Discovered)
            return false;
        firstBellStage = discovery ? FirstBellStage.Discovered : FirstBellStage.Inspected;
        explorationFrame = Time.frameCount;
        return true;
    }

    public void RecordFirstBell() => firstBellStage = FirstBellStage.Used;
    public void CompleteTutorial() => tutorialExited = true;

    public static void ResetForMenu()
    {
        bool changed = memoryLayer;
        hasLantern = false;
        memoryLayer = false;
        firstBellStage = FirstBellStage.Unexplored;
        storyACompleted = false;
        tutorialExited = false;
        explorationFrame = -1;
        if (changed && Instance != null) Instance.OnWorldSwitched?.Invoke(false);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
