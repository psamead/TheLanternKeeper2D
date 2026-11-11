using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds the placeholder lantern pickup, Storyboard A overlay, and EndCard B flow.</summary>
internal static class LanternFlowSceneBuilder
{
    private const string ProloguePath = "Assets/_Project/Scenes/00b_Prologue.unity";
    private const string CorePath = "Assets/_Project/Scenes/Core_Persistent.unity";
    private const string HomePath = "Assets/_Project/Scenes/01_Home.unity";
    private const string EndPath = "Assets/_Project/Scenes/03_EndCard.unity";

    [MenuItem("Tools/Lantern Keeper/Build Lantern Pickup and EndCard Flow")]
    private static void Build()
    {
        var core = GetScene(CorePath);
        var home = GetScene(HomePath);
        var end = GetScene(EndPath);
        var prologue = GetScene(ProloguePath);
        var flow = AssetDatabase.LoadAssetAtPath<LanternFlowText>("Assets/_Project/Data/Storyboard/LanternFlowText.asset");
        var storyA = AssetDatabase.LoadAssetAtPath<StoryboardData>("Assets/_Project/Data/Storyboard/SB_LanternOrigin_A.asset");
        var storyB = AssetDatabase.LoadAssetAtPath<StoryboardData>("Assets/_Project/Data/Storyboard/SB_Ending_B.asset");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Cinzel SDF Generated.asset");
        var menuFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Cinzel SDF Generated.asset");
        if (!flow || !storyA || !storyB || !font || !menuFont)
            throw new System.Exception("Required storyboard, text, or font asset is missing.");

        BuildCore(core, prologue, storyA, flow, font);
        BuildHome(home, core, flow, font);
        BuildEndCard(end, prologue, storyB, flow, menuFont);
        EditorSceneManager.MarkSceneDirty(core);
        EditorSceneManager.MarkSceneDirty(home);
        EditorSceneManager.MarkSceneDirty(end);
        EditorSceneManager.SaveScene(core);
        EditorSceneManager.SaveScene(home);
        EditorSceneManager.SaveScene(end);
        Debug.Log("Lantern pickup + Storyboard A + EndCard B scenes saved.");
    }

    private static Scene GetScene(string path)
    {
        var scene = SceneManager.GetSceneByPath(path);
        return scene.IsValid() && scene.isLoaded ? scene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }

    private static GameObject Root(Scene scene, string name) => scene.GetRootGameObjects().FirstOrDefault(go => go.name == name);

    private static void SetRef(Component component, string field, Object value)
    {
        var serialized = new SerializedObject(component);
        var property = serialized.FindProperty(field);
        if (property == null) throw new System.Exception(component.GetType().Name + " is missing field " + field);
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(component);
    }

    private static void MoveTo(GameObject go, Scene scene)
    {
        if (go.scene != scene) SceneManager.MoveGameObjectToScene(go, scene);
    }

    private static GameObject CloneCanvas(Scene source, Scene target, string name, int sortingOrder)
    {
        var sourceCanvas = Root(source, "Prologue Canvas");
        if (!sourceCanvas) throw new System.Exception("Prologue Canvas missing.");
        var clone = Object.Instantiate(sourceCanvas);
        clone.name = name;
        MoveTo(clone, target);
        clone.GetComponent<Canvas>().sortingOrder = sortingOrder;
        clone.transform.Find("BlackFade").GetComponent<CanvasGroup>().alpha = 1f;
        return clone;
    }

    private static StoryboardPlayer ClonePlayer(Scene source, Scene target, GameObject canvasRoot, StoryboardData data, string name, bool fadeBack)
    {
        var sourceRoot = Root(source, "StoryboardPlayer");
        if (!sourceRoot || !sourceRoot.GetComponent<StoryboardPlayer>()) throw new System.Exception("Prologue StoryboardPlayer missing.");
        var clone = Object.Instantiate(sourceRoot);
        clone.name = name;
        MoveTo(clone, target);
        var player = clone.GetComponent<StoryboardPlayer>();
        player.data = data;
        player.playOnStart = false;
        player.fadeBackAtEnd = fadeBack;
        player.onFinished = null;
        player.layerA = canvasRoot.transform.Find("LayerA").GetComponent<RawImage>();
        player.layerB = canvasRoot.transform.Find("LayerB").GetComponent<RawImage>();
        var caption = canvasRoot.transform.Find("Caption Group");
        player.captionGroup = caption.GetComponent<CanvasGroup>();
        player.captionText = caption.GetComponentInChildren<TMP_Text>(true);
        player.blackFade = canvasRoot.transform.Find("BlackFade").GetComponent<CanvasGroup>();
        return player;
    }

    private static TMP_Text CreateWorldText(string name, Transform parent, Vector3 localPosition, string value, TMP_FontAsset font, float size, int order)
    {
        var go = new GameObject(name, typeof(TextMeshPro));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        var text = go.GetComponent<TextMeshPro>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(240f / 255f, 238f / 255f, 209f / 255f, 1f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.GetComponent<MeshRenderer>().sortingOrder = order;
        return text;
    }

    private static void BuildCore(Scene core, Scene prologue, StoryboardData storyA, LanternFlowText flow, TMP_FontAsset font)
    {
        var playerRoot = Root(core, "Player");
        if (!playerRoot) throw new System.Exception("Core Player root missing.");
        if (Root(core, "StoryboardA_Player")) return;
        var corePlaceholder = Root(core, "Canvas")?.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(text => text.name == "PlaceholderLabel");
        if (corePlaceholder) corePlaceholder.gameObject.SetActive(false);
        Undo.RegisterFullObjectHierarchyUndo(playerRoot, "Configure lantern visibility");
        var lantern = playerRoot.transform.Find("Maela/Hip/Body/Shoulder_R/Elbow_R/Wrist_R/Lantern");
        if (!lantern) throw new System.Exception("Maela hand lantern path missing.");
        var handSprite = lantern.Find("Lantern_Sprite").GetComponent<SpriteRenderer>();
        var handLight = lantern.Find("LanternLight").GetComponent<Light2D>();
        handSprite.enabled = false;
        handLight.enabled = false;
        var visual = playerRoot.GetComponent<LanternVisualState>() ?? Undo.AddComponent<LanternVisualState>(playerRoot);
        SetRef(visual, "lanternSprite", handSprite);
        SetRef(visual, "lanternLight", handLight);

        var canvas = CloneCanvas(prologue, core, "StoryboardA_Canvas", 80);
        canvas.SetActive(false);
        ClonePlayer(prologue, core, canvas, storyA, "StoryboardA_Player", true);
        var coreCanvas = Root(core, "Canvas");
        var fade = new GameObject("EndFade_TEMP", typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
        MoveTo(fade, core);
        fade.transform.SetParent(coreCanvas.transform, false);
        var fadeRect = fade.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero; fadeRect.anchorMax = Vector2.one; fadeRect.offsetMin = Vector2.zero; fadeRect.offsetMax = Vector2.zero;
        var fadeImage = fade.GetComponent<UnityEngine.UI.Image>(); fadeImage.color = Color.black; fadeImage.raycastTarget = false;
        fade.GetComponent<CanvasGroup>().alpha = 0f;
        coreCanvas.transform.SetAsLastSibling();
        coreCanvas.GetComponent<Canvas>().sortingOrder = 20;

        var hintRoot = new GameObject("LanternProgressHint");
        MoveTo(hintRoot, core);
        var hint = hintRoot.AddComponent<LanternProgressHint>();
        var hintText = CreateWorldText("HintText", hintRoot.transform, Vector3.zero, flow.raiseHint, font, 4f, 50);
        SetRef(hint, "hint", hintText); SetRef(hint, "followTarget", playerRoot.transform); SetRef(hint, "flowText", flow);
        hintRoot.transform.position = playerRoot.transform.position + Vector3.up * 1.8f;
    }

    private static void BuildHome(Scene home, Scene core, LanternFlowText flow, TMP_FontAsset font)
    {
        if (Root(home, "Lantern_Pickup") && Root(home, "EndTrigger_TEMP")) return;
        if (Root(home, "Lantern_Pickup") || Root(home, "EndTrigger_TEMP"))
            throw new System.Exception("Home has only part of the lantern flow; builder stopped to preserve the scene.");
        var corePlayer = Root(core, "Player");
        var source = corePlayer.transform.Find("Maela/Hip/Body/Shoulder_R/Elbow_R/Wrist_R/Lantern/Lantern_Sprite").GetComponent<SpriteRenderer>();
        var pickup = new GameObject("Lantern_Pickup", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(LanternPickupPulse), typeof(LanternPickupInteractable));
        MoveTo(pickup, home);
        var sprite = pickup.GetComponent<SpriteRenderer>();
        sprite.sprite = source.sprite; sprite.sharedMaterial = source.sharedMaterial; sprite.sortingLayerID = source.sortingLayerID; sprite.sortingOrder = 20;
        Vector3 scale = source.transform.lossyScale;
        pickup.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);
        float height = source.sprite.bounds.size.y * Mathf.Abs(scale.y);
        pickup.transform.position = new Vector3(-3f, -3.2f + height * .48f, 0f);
        var light = pickup.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point; light.color = new Color(1f, .68f, .29f, 1f); light.intensity = .7f;
        light.pointLightOuterRadius = 1.45f; light.pointLightInnerRadius = .4f; light.falloffIntensity = .65f;
        SetRef(pickup.GetComponent<LanternPickupPulse>(), "targetLight", light);
        var collider = pickup.GetComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = .75f;
        var prompt = CreateWorldText("PickupPrompt", pickup.transform, new Vector3(0f, Mathf.Max(.75f, height * .9f), 0f), flow.pickupPrompt, font, 4f, 25);
        prompt.transform.SetParent(null, true);
        prompt.transform.localScale = Vector3.one;
        prompt.transform.position = pickup.transform.position + Vector3.up * (height + .2f);
        SetRef(pickup.GetComponent<LanternPickupInteractable>(), "prompt", prompt);
        SetRef(pickup.GetComponent<LanternPickupInteractable>(), "flowText", flow);

        // TEMP: this finish volume moves to the attic window after the memory layer is built.
        var finish = new GameObject("EndTrigger_TEMP", typeof(BoxCollider2D), typeof(EndTrigger_TEMP));
        MoveTo(finish, home); finish.transform.position = new Vector3(5.35f, -2.4f, 0f);
        var box = finish.GetComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(1.1f, 2.2f);
    }

    private static void BuildEndCard(Scene end, Scene prologue, StoryboardData storyB, LanternFlowText flow, TMP_FontAsset menuFont)
    {
        if (Root(end, "StoryboardB_Player"))
        {
            WireExistingEndCardButtons(end);
            return;
        }
        var placeholder = Root(end, "Canvas")?.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "PlaceholderLabel");
        if (placeholder) placeholder.gameObject.SetActive(false);
        var canvas = CloneCanvas(prologue, end, "StoryboardB_Canvas", 0);
        var player = ClonePlayer(prologue, end, canvas, storyB, "StoryboardB_Player", false);
        var musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Prologue/BGM_Prologue_ArMorDu_loop_v1.ogg");
        var musicRoot = new GameObject("EndCardPrologueMusic", typeof(AudioSource));
        MoveTo(musicRoot, end);
        var music = musicRoot.GetComponent<AudioSource>();
        music.clip = musicClip; music.loop = true; music.playOnAwake = true; music.volume = .6f; music.spatialBlend = 0f;
        var flowRoot = new GameObject("EndCardFlow"); MoveTo(flowRoot, end);
        var controller = flowRoot.AddComponent<EndCardController>();
        var buttons = new GameObject("EndCardButtons", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        buttons.transform.SetParent(canvas.transform, false);
        var panel = buttons.GetComponent<RectTransform>(); panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f); panel.pivot = new Vector2(.5f, .5f); panel.anchoredPosition = new Vector2(0, 220); panel.sizeDelta = new Vector2(700, 220);
        buttons.GetComponent<UnityEngine.UI.Image>().color = new Color(.025f, .045f, .075f, .90f);
        var back = CreateButton(buttons.transform, "BackToMenuButton", menuFont, new Vector2(0, 48), new Vector2(480, 68), out var backText);
        var quit = CreateButton(buttons.transform, "QuitButton", menuFont, new Vector2(0, -44), new Vector2(480, 68), out var quitText);
        UnityEventTools.AddPersistentListener(back.onClick, controller.BackToMenu);
        UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
        buttons.SetActive(false);
        SetRef(controller, "storyboard", player); SetRef(controller, "buttonsRoot", buttons);
        SetRef(controller, "backLabel", backText); SetRef(controller, "quitLabel", quitText); SetRef(controller, "flowText", flow);
    }

    private static void WireExistingEndCardButtons(Scene end)
    {
        var controller = Root(end, "EndCardFlow")?.GetComponent<EndCardController>();
        if (!controller) throw new System.Exception("Existing EndCardFlow controller missing.");
        foreach (var buttonName in new[] { "BackToMenuButton", "QuitButton" })
        {
            var button = Root(end, "StoryboardB_Canvas")?.GetComponentsInChildren<Button>(true).FirstOrDefault(item => item.name == buttonName);
            if (!button) throw new System.Exception("Existing EndCard button missing: " + buttonName);
            if (button.onClick.GetPersistentEventCount() == 0)
            {
                Undo.RecordObject(button, "Wire EndCard button");
                UnityEventTools.AddPersistentListener(button.onClick, buttonName == "BackToMenuButton" ? controller.BackToMenu : controller.Quit);
                EditorUtility.SetDirty(button);
            }
        }
    }

    private static Button CreateButton(Transform parent, string name, TMP_FontAsset font, Vector2 position, Vector2 size, out TMP_Text label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
        var image = go.GetComponent<UnityEngine.UI.Image>(); image.color = new Color(18f / 255f, 31f / 255f, 46f / 255f, .96f);
        var button = go.GetComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1f, .88f, .62f, 1f); colors.pressedColor = new Color(.78f, .71f, .55f, 1f); button.colors = colors;
        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.GetComponent<RectTransform>(); textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = new Vector2(12, 6); textRect.offsetMax = new Vector2(-12, -6);
        label = textGO.GetComponent<TMP_Text>(); label.font = font; label.fontSize = 32; label.alignment = TextAlignmentOptions.Center; label.color = new Color(240f / 255f, 238f / 255f, 209f / 255f, 1f);
        return button;
    }
}
