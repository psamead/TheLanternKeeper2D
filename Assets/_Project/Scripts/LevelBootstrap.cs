using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Supports playing a level opened directly, without duplicating Core.</summary>
public sealed class LevelBootstrap : MonoBehaviour
{
    private IEnumerator Start()
    {
        if (SceneManager.GetSceneByName("Core_Persistent").isLoaded) yield break;
        LevelLoader.SetInitialLevel(gameObject.scene.name, "Spawn_Start");
        yield return SceneManager.LoadSceneAsync("Core_Persistent", LoadSceneMode.Additive);
    }
}
