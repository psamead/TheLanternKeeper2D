using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void Load(string sceneName)
    {
        if (sceneName == "01_Home")
        {
            if (LevelLoader.Instance != null)
                LevelLoader.Instance.LoadLevel(sceneName, "Spawn_Start");
            else
            {
                var prologue = Object.FindFirstObjectByType<PrologueAudio>();
                if (prologue != null) prologue.HandOffToCore();
                LevelLoader.SetInitialLevel(sceneName, "Spawn_Start");
                SceneManager.LoadScene("Core_Persistent", LoadSceneMode.Single);
            }
            return;
        }
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (sceneName == "03_EndCard" && AudioManager.Instance)
                AudioManager.Instance.CaptureForEndCard();
            SceneManager.LoadScene(sceneName);
        }
        else
            UnityEngine.Debug.LogError($"Scene '{sceneName}' is not in Build Settings.");
    }
}
