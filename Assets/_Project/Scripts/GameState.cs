using UnityEngine;

/// <summary>Session state owned by the loaded Core scene.</summary>
public sealed class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    public bool HasLantern { get; set; }
    public bool MemoryLayer { get; set; }

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
