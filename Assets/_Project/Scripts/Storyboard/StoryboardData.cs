using UnityEngine;
using UnityEngine.Video;

// One storyboard sequence (Prologue P / A / B). Text lives here, not in code.
[CreateAssetMenu(menuName = "Lantern Keeper/Storyboard", fileName = "SB_New")]
public class StoryboardData : ScriptableObject
{
    [System.Serializable]
    public class Frame
    {
        public Sprite image;                       // still image (Ken Burns)
        public VideoClip video;                    // optional; if set, plays instead of the image (Flow shots)
        [TextArea(2, 4)] public string caption;
        public float duration = 6f;                // ignored for video (uses clip length, capped by duration if > 0)
        [Header("Ken Burns (scale, normalized pan -1..1)")]
        public float zoomFrom = 1.0f, zoomTo = 1.12f;
        public Vector2 panFrom = Vector2.zero, panTo = Vector2.zero;
    }
    public Frame[] frames;
    public float crossFade = 1.0f;
    public string nextScene = "01_Home";           // empty = just finish (event only)
}
