using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class BackToMenu : MonoBehaviour
{
    private void Update()
    {
        if (StoryboardPlayer.AnyPlaybackActive || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (SceneManager.GetActiveScene().name == "03_EndCard")
            return;

        GameState.ResetForMenu();
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.ReturnToMenu();
        }
        else SceneLoader.Load("00_StartMenu");
    }
}
