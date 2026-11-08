using UnityEngine;
using UnityEngine.Video;
using TMPro;

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
        [Header("Caption layout (screen-normalized anchors)")]
        public Vector2 anchorMin = new Vector2(0.12239583f, 0.09259259f);
        public Vector2 anchorMax = new Vector2(0.8776042f, 0.24074075f);
        public TextAlignmentOptions align = TextAlignmentOptions.Center;
        public bool wordWrap = true;
        [Min(0.01f)] public float fontScale = 1f;
        [Header("Caption appearance")]
        public Color captionColor = Color.white;
        public bool overrideCaptionColor;
        public bool enableUnderlay;
        public Color underlayColor = new Color(11f / 255f, 26f / 255f, 46f / 255f, 0.6f);
        public Vector2 underlayOffset = new Vector2(0f, -0.5f);
        public float underlayDilate = 0.2f;
        public float underlaySoftness = 0.6f;
        [Header("Ken Burns (scale, normalized pan -1..1)")]
        public float zoomFrom = 1.0f, zoomTo = 1.12f;
        public Vector2 panFrom = Vector2.zero, panTo = Vector2.zero;
    }
    public Frame[] frames;
    public float crossFade = 1.0f;
    public string nextScene = "01_Home";           // empty = just finish (event only)
}
