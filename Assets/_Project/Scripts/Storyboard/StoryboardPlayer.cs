using System.Collections;
using System.Collections.Generic;
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
    public bool fadeBackAtEnd;
    public System.Action onFinished;

    public static bool AnyPlaybackActive { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool WasSkipped { get; private set; }

    bool skipFrame, skipAll; RawImage front, back;
    float baseCaptionFontSize;
    Color baseCaptionColor;
    Material baseCaptionMaterial;
    readonly Dictionary<StoryboardData.Frame, Material> captionMaterials = new Dictionary<StoryboardData.Frame, Material>();

    void Awake()
    {
        baseCaptionFontSize = captionText.fontSize;
        baseCaptionColor = captionText.color;
        baseCaptionMaterial = captionText.fontSharedMaterial;
    }

    void Start()
    {
        if (playOnStart) StartCoroutine(Play());
    }

    void OnDestroy()
    {
        foreach (var material in captionMaterials.Values)
            if (material) Destroy(material);
        captionMaterials.Clear();
    }

    void Update()
    {
        var k = Keyboard.current; var m = Mouse.current;
        if (k != null && k.escapeKey.wasPressedThisFrame) skipAll = true;
        else if ((k != null && k.anyKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame)) skipFrame = true;
    }

    public IEnumerator Play()
    {
        skipAll = false;
        skipFrame = false;
        WasSkipped = false;
        IsPlaying = true;
        AnyPlaybackActive = true;
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
            ApplyCaptionLayout(f);
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
            if (!isVideo) KenBurns(front, f, 1f);
#if UNITY_EDITOR
            if (data != null && data.name == "SB_Prologue" && i > 0)
            {
                ValidateCaptionBounds(f, i);
                if (i > 0 && i < 3 && UnityEditor.EditorPrefs.GetBool("PrologueCaptionLayout.CaptureEnabled", false))
                    yield return CaptureCaptionLayoutScreenshot(i);
            }
#endif
            if (isVideo) videoPlayer.Stop();
            captionGroup.alpha = 0;
        }
        yield return Fade(blackFade, blackFade.alpha, 1, 0.8f);
        if (fadeBackAtEnd)
            yield return Fade(blackFade, 1, 0, 0.6f);
        IsPlaying = false;
        AnyPlaybackActive = false;
        WasSkipped = skipAll;
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
    void ApplyCaptionLayout(StoryboardData.Frame frame)
    {
        RectTransform rect = captionText.rectTransform;
        rect.anchorMin = frame.anchorMin;
        rect.anchorMax = frame.anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        captionText.alignment = frame.align;
        captionText.enableWordWrapping = frame.wordWrap;
        captionText.fontSize = baseCaptionFontSize * Mathf.Max(0.01f, frame.fontScale);
        captionText.color = frame.overrideCaptionColor ? frame.captionColor : baseCaptionColor;

        if (!frame.enableUnderlay)
        {
            captionText.fontSharedMaterial = baseCaptionMaterial;
            return;
        }

        if (!captionMaterials.TryGetValue(frame, out var material) || !material)
        {
            material = new Material(baseCaptionMaterial) { name = "Storyboard Caption Underlay" };
            material.EnableKeyword("UNDERLAY_ON");
            captionMaterials[frame] = material;
        }
        material.SetColor("_UnderlayColor", frame.underlayColor);
        material.SetFloat("_UnderlayOffsetX", frame.underlayOffset.x);
        material.SetFloat("_UnderlayOffsetY", frame.underlayOffset.y);
        material.SetFloat("_UnderlayDilate", frame.underlayDilate);
        material.SetFloat("_UnderlaySoftness", frame.underlaySoftness);
        captionText.fontMaterial = material;
    }
#if UNITY_EDITOR
    void ValidateCaptionBounds(StoryboardData.Frame frame, int frameIndex)
    {
        captionText.ForceMeshUpdate();
        Bounds bounds = captionText.textBounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] localCorners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, max.y, min.z)
        };
        RectTransform rect = captionText.rectTransform;
        Canvas canvas = captionText.canvas;
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        Vector2 screenMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 screenMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool allCornersInside = bounds.size.x > 0f && bounds.size.y > 0f;
        for (int cornerIndex = 0; cornerIndex < localCorners.Length; cornerIndex++)
        {
            Vector3 world = rect.TransformPoint(localCorners[cornerIndex]);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(eventCamera, world);
            Vector2 normalized = new Vector2(screen.x / Screen.width, screen.y / Screen.height);
            screenMin = Vector2.Min(screenMin, normalized);
            screenMax = Vector2.Max(screenMax, normalized);
            bool inside = normalized.x >= frame.anchorMin.x - 0.001f && normalized.x <= frame.anchorMax.x + 0.001f &&
                          normalized.y >= frame.anchorMin.y - 0.001f && normalized.y <= frame.anchorMax.y + 0.001f;
            allCornersInside &= inside;
        }
        string report = $"CAPTION_BOUNDS {(allCornersInside ? "PASS" : "FAIL")} P{frameIndex + 1}: " +
                  $"box=({frame.anchorMin.x:F3},{frame.anchorMin.y:F3})-({frame.anchorMax.x:F3},{frame.anchorMax.y:F3}) " +
                  $"text=({screenMin.x:F3},{screenMin.y:F3})-({screenMax.x:F3},{screenMax.y:F3}) " +
                  $"font={captionText.fontSize:F1} lines={captionText.textInfo.lineCount}";
        if (UnityEditor.EditorPrefs.GetBool("PrologueCaptionLayout.CaptureEnabled", false))
        {
            string captureDirectory = UnityEditor.EditorPrefs.GetString("PrologueCaptionLayout.CaptureDirectory", "");
            if (!string.IsNullOrEmpty(captureDirectory))
            {
                System.IO.Directory.CreateDirectory(captureDirectory);
                string reportPath = System.IO.Path.Combine(captureDirectory, "CaptionBounds.txt");
                if (frameIndex == 1)
                    System.IO.File.WriteAllText(reportPath, string.Empty);
                System.IO.File.AppendAllText(reportPath, report + System.Environment.NewLine);
            }
        }
    }
    System.Collections.IEnumerator CaptureCaptionLayoutScreenshot(int frameIndex)
    {
        yield return new WaitForEndOfFrame();
        string captureDirectory = UnityEditor.EditorPrefs.GetString("PrologueCaptionLayout.CaptureDirectory", "");
        if (!string.IsNullOrEmpty(captureDirectory))
        {
            System.IO.Directory.CreateDirectory(captureDirectory);
            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(captureDirectory, $"P{frameIndex + 1}_caption_v3.png"), screenshot.EncodeToPNG());
            Destroy(screenshot);
        }
        if (frameIndex == 2)
            UnityEditor.EditorPrefs.SetBool("PrologueCaptionLayout.CaptureEnabled", false);
    }
#endif
    static void SetAlpha(Graphic g, float a) { var c = g.color; c.a = a; g.color = c; }
    static IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    { for (float t = 0; t < d; t += Time.deltaTime) { g.alpha = Mathf.Lerp(a, b, t / d); yield return null; } g.alpha = b; }
}
