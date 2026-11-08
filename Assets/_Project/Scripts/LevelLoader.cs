using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Keeps Core loaded while attaching one additive gameplay level.</summary>
public sealed class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    [SerializeField] private PlayerController player;
    [SerializeField] private CinemachineCamera gameplayCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private Light2D globalLight;
    private Scene currentLevel;
    private bool loading;
    private static string initialScene = "01_Home", initialSpawn = "Spawn_Start";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInitialLevel() { initialScene = "01_Home"; initialSpawn = "Spawn_Start"; }

    public static void SetInitialLevel(string sceneName, string spawnId)
    { initialScene = sceneName; initialSpawn = spawnId; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.enabled = false;
    }

    private void Start() => LoadLevel(initialScene, initialSpawn);

    public void LoadLevel(string sceneName, string spawnId)
    {
        if (loading) return;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        { Debug.LogError($"Level '{sceneName}' is not in Build Settings.", this); return; }
        StartCoroutine(LoadRoutine(sceneName, spawnId));
    }

    private IEnumerator LoadRoutine(string sceneName, string spawnId)
    {
        loading = true;
        var body = player.GetComponent<Rigidbody2D>();
        player.enabled = false;
        body.simulated = false;
        Scene next = SceneManager.GetSceneByName(sceneName);
        if (!next.isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            next = SceneManager.GetSceneByName(sceneName);
        }
        LevelInfo info = null;
        SpawnPoint spawn = null;
        foreach (var root in next.GetRootGameObjects())
        {
            if (info == null) info = root.GetComponentInChildren<LevelInfo>(true);
            foreach (var candidate in root.GetComponentsInChildren<SpawnPoint>(true))
                if (candidate.id == spawnId) spawn = candidate;
        }
        if (info == null || info.cameraBounds == null || spawn == null)
        {
            Debug.LogError($"Level '{sceneName}' requires LevelInfo, camera bounds and spawn '{spawnId}'.", this);
            loading = false;
            yield break;
        }
        SceneManager.SetActiveScene(next);
        player.transform.position = spawn.transform.position;
        body.position = spawn.transform.position;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        gameplayCamera.Follow = player.transform;
        var lens = gameplayCamera.Lens;
        lens.OrthographicSize = info.orthoSize;
        gameplayCamera.Lens = lens;
        confiner.BoundingShape2D = info.cameraBounds;
        confiner.InvalidateBoundingShapeCache();
        gameplayCamera.PreviousStateIsValid = false;
        globalLight.color = info.globalLightColor;
        globalLight.intensity = info.globalLightIntensity;
        if (info.musicClip != null || !info.keepMusicWhenNull)
            AudioManager.Instance.PlayMusic(info.musicClip, info.musicVolume, info.audioFadeSeconds);
        AudioManager.Instance.PlayAmbience(info.ambienceClip, info.ambienceVolume, info.audioFadeSeconds);
        Scene previous = currentLevel;
        currentLevel = next;
        if (previous.IsValid() && previous.isLoaded && previous != next)
            yield return SceneManager.UnloadSceneAsync(previous);
        Physics2D.SyncTransforms();
        body.simulated = true;
        player.enabled = true;
        loading = false;
    }

    public void ReturnToMenu()
    {
        if (loading) return;
        loading = true;
        // Single loading unloads both Core and its additive level together.
        SceneManager.LoadScene("00_StartMenu", LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
