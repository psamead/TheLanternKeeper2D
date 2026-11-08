using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BackToMenu : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (LevelLoader.Instance != null) LevelLoader.Instance.ReturnToMenu();
            else SceneLoader.Load("00_StartMenu");
        }
    }
}
