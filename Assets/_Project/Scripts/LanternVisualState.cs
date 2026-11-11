using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class LanternVisualState : MonoBehaviour
{
    [SerializeField] private SpriteRenderer lanternSprite;
    [SerializeField] private Light2D lanternLight;

    private void Start() => SetVisible(GameState.Instance != null && GameState.Instance.HasLantern);

    public void SetVisible(bool visible)
    {
        if (lanternSprite) lanternSprite.enabled = visible;
        if (lanternLight) lanternLight.enabled = visible;
    }
}
