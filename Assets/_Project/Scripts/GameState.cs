using UnityEngine;
using System;

/// <summary>Session state owned by the loaded Core scene.</summary>
public sealed class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    private static bool hasLantern;
    private static bool memoryLayer;
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
    private static void ResetSessionState() { hasLantern = false; memoryLayer = false; Instance = null; }

    public static void ResetForMenu()
    {
        bool changed = memoryLayer;
        hasLantern = false;
        memoryLayer = false;
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
