using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

// Full-screen storyboard: Ken Burns on stills, optional video frames, cross-fade, captions.
// Any key / click = next frame; Esc = skip all. Loads data.nextScene at the end.
public class StoryboardPlayer : MonoBehaviour
{
    public StoryboardData data;
    [Header("UI (two image layers for cross-fade)")]
    public RawImage layerA, layerB;              // full-screen, stretched, Aspect handled by uvRect
    public CanvasGroup captionGroup;
    public TMP_Text captionText;
    public CanvasGroup blackFade;                // full-screen black, starts at 1
    public VideoPlayer videoPlayer;              // Render Mode = Render Texture
    public RenderTexture videoTexture;           // 1920x1080
    public bool playOnStart = true;
    public System.Action onFinished;

    bool skipFrame, skipAll; RawImage front, back;

    void Start() { if (playOnStart) StartCoroutine(Play()); }

    void Update()
    {
        var k = Keyboard.current; var m = Mouse.current;
        if (k != null && k.escapeKey.wasPressedThisFrame) skipAll = true;
        else if ((k != null && k.anyKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame)) skipFrame = true;
    }

    public IEnumerator Play()
    {
        front = layerA; back = layerB;
        SetAlpha(front, 0); SetAlpha(back, 0); captionGroup.alpha = 0;
        yield return Fade(blackFade, 1, 0, 0.6f);
        for (int i = 0; i < data.frames.Length && !skipAll; i++)
        {
            var f = data.frames[i]; skipFrame = false;
            bool isVideo = f.video != null && videoPlayer != null;
            if (isVideo)
            {
                videoPlayer.clip = f.video; videoPlayer.targetTexture = videoTexture; videoPlayer.Prepare();
                while (!videoPlayer.isPrepared) yield return null;
                back.texture = videoTexture; back.uvRect = new Rect(0, 0, 1, 1); videoPlayer.Play();
            }
            else back.texture = f.image ? f.image.texture : null;

            float len = isVideo ? Mathf.Min((float)f.video.length, f.duration > 0 ? Mathf.Max(f.duration, 1f) : 999f) : f.duration;
            captionText.text = f.caption;
            // cross-fade in
            for (float t = 0; t < data.crossFade && !skipAll; t += Time.deltaTime)
            {
                float u = t / data.crossFade;
                SetAlpha(back, u); SetAlpha(front, 1 - u); captionGroup.alpha = Mathf.Clamp01(u * 1.5f - 0.5f);
                if (!isVideo) KenBurns(back, f, t / len);
                yield return null;
            }
            SetAlpha(back, 1); SetAlpha(front, 0); captionGroup.alpha = 1;
            (front, back) = (back, front);
            for (float t = data.crossFade; t < len && !skipFrame && !skipAll; t += Time.deltaTime)
            {
                if (!isVideo) KenBurns(front, f, t / len);
                yield return null;
            }
            if (isVideo) videoPlayer.Stop();
            captionGroup.alpha = 0;
        }
        yield return Fade(blackFade, blackFade.alpha, 1, 0.8f);
        onFinished?.Invoke();
        if (!string.IsNullOrEmpty(data.nextScene)) SceneLoader.Load(data.nextScene);
    }

    static void KenBurns(RawImage img, StoryboardData.Frame f, float u)
    {
        if (img.texture == null) return;
        u = Mathf.Clamp01(u); u = u * u * (3 - 2 * u);
        float z = Mathf.Lerp(f.zoomFrom, f.zoomTo, u);
        Vector2 pan = Vector2.Lerp(f.panFrom, f.panTo, u);
        // fit image aspect into 16:9 screen (cover), then zoom
        float texAspect = (float)img.texture.width / img.texture.height, scr = 16f / 9f;
        float w = 1f / z, h = 1f / z;
        if (texAspect > scr) w *= scr / texAspect; else h *= texAspect / scr;
        float x = 0.5f - w / 2 + pan.x * (1 - w) / 2, y = 0.5f - h / 2 + pan.y * (1 - h) / 2;
        img.uvRect = new Rect(x, y, w, h);
    }
    static void SetAlpha(Graphic g, float a) { var c = g.color; c.a = a; g.color = c; }
    static IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    { for (float t = 0; t < d; t += Time.deltaTime) { g.alpha = Mathf.Lerp(a, b, t / d); yield return null; } g.alpha = b; }
}
