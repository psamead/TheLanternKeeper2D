using UnityEngine;
using UnityEngine.UI;

/// Scrolls a RawImage texture horizontally (seamless fog). Texture Wrap Mode must be Repeat.
/// speedPixelsPerSecond is in reference-screen pixels (1920 wide), so it reads like the preview.
[RequireComponent(typeof(RawImage))]
public sealed class UVScroll : MonoBehaviour
{
    [SerializeField] private float speedPixelsPerSecond = 14f;  // + = drift to the right
    [SerializeField, Min(1f)] private float displayWidth = 2400f; // how wide one texture repeat is on screen

    private RawImage image;

    private void Awake() => image = GetComponent<RawImage>();

    private void Update()
    {
        Rect r = image.uvRect;
        r.x -= speedPixelsPerSecond / displayWidth * Time.unscaledDeltaTime;
        r.x = Mathf.Repeat(r.x, 1f);
        image.uvRect = r;
    }
}
