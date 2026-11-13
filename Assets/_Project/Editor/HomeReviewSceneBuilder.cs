using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>Applies the Home art and presentation to the existing scenes.</summary>
public static class HomeReviewSceneBuilder
{
    private const string Art = "Assets/_Project/Art/Scenes/Home/";
    private const string Home = "Assets/_Project/Scenes/01_Home.unity";
    private const string Core = "Assets/_Project/Scenes/Core_Persistent.unity";
    private const string End = "Assets/_Project/Scenes/03_EndCard.unity";
    private const string Menu = "Assets/_Project/Scenes/00_StartMenu.unity";

    [MenuItem("Tools/Lantern Keeper/Apply Home Review Art and Flow")]
    public static void ApplyAll()
    {
        ImportSprites();
        UpdateFlowText();
        UpdateMaelaPrefab();
        UpdateCore();
        UpdateHome();
        UpdateEndCard();
        EditorSceneManager.OpenScene(Menu, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("Home review art and flow applied. Edit Mode returned to 00_StartMenu.");
    }

    private static void ImportSprites()
    {
        ImportSprite(Art + "Home_Present_v1.png", false, 4096);
        ImportSprite(Art + "Home_Present_v3.png", false, 4096);
        ImportSprite(Art + "Attic_PerspectiveRef_v3.png", false, 4096);
        ImportSprite(Art + "Ladder_Whole_v5.png", true, 8192);
        ImportSprite(Art + "Lantern_v4.png", true, 4096);
    }

    private static void ImportSprite(string path, bool transparency, int maxSize)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!importer) throw new InvalidOperationException("Missing selected art: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = transparency;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = maxSize;
        importer.SaveAndReimport();
    }

    private static void UpdateFlowText()
    {
        var text = AssetDatabase.LoadAssetAtPath<LanternFlowText>("Assets/_Project/Data/Storyboard/LanternFlowText.asset");
        if (!text) throw new InvalidOperationException("LanternFlowText asset is missing.");
        Undo.RecordObject(text, "Update Home interaction text");
        text.pickupPrompt = "Press T";
        text.raiseHint = "Press E";
        text.brokenLadderHint = "Broken ladder";
        EditorUtility.SetDirty(text);
    }

    private static void UpdateMaelaPrefab()
    {
        const string path = "Assets/_Project/Prefabs/Maela.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var lantern = FindDeep(root.transform, "Lantern_Sprite");
            if (!lantern) throw new InvalidOperationException("Maela lantern sprite child is missing.");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Lantern_v4.png");
            var renderer = lantern.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            // The Core scene scales Maela's visual to 1.4. This yields a 0.70-unit held lantern.
            lantern.localScale = Vector3.one * 0.02667f;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void UpdateCore()
    {
        EditorSceneManager.OpenScene(Core, OpenSceneMode.Single);
        var text = FindSceneObject("HintText")?.GetComponent<TMP_Text>();
        var hint = FindSceneObject("LanternProgressHint")?.GetComponent<LanternProgressHint>();
        if (!text || !hint) throw new InvalidOperationException("Core shared hint is missing.");
        Undo.RecordObject(text, "Shrink shared world prompt");
        text.fontSize = 2.15f;
        SetSerialized(hint, "worldOffset", new Vector3(0f, 2.42f, 0f));
        var frame = text.GetComponent<RoundedWorldPromptFrame>();
        if (frame)
        {
            SetSerialized(frame, "padding", 0.06f);
            SetSerialized(frame, "lineWidth", 0.01f);
            SetSerialized(frame, "radius", 0.05f);
        }
        SaveActiveScene();
    }

    private static void UpdateHome()
    {
        EditorSceneManager.OpenScene(Home, OpenSceneMode.Single);
        var present = FindSceneObject("Background")?.GetComponent<SpriteRenderer>();
        var memory = FindSceneObject("Home_Memory_bg")?.GetComponent<SpriteRenderer>();
        var level = FindSceneObject("LevelInfo")?.GetComponent<LevelInfo>();
        var roomBounds = FindSceneObject("CameraBounds")?.GetComponent<PolygonCollider2D>();
        var attic = FindSceneObject("Attic_Greybox");
        var whole = FindSceneObject("Ladder_Whole");
        var pickup = FindSceneObject("Lantern_Pickup");
        var endTrigger = FindSceneObject("EndTrigger_AtticWindow");
        if (!present || !memory || !level || !roomBounds || !attic || !whole || !pickup || !endTrigger)
            throw new InvalidOperationException("Home review requires the current Home scene objects.");

        var presentSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Home_Present_v1.png");
        var memorySprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Home_Present_v3.png");
        Undo.RecordObject(present, "Replace Home art");
        Undo.RecordObject(memory, "Replace Memory Home art");
        present.sprite = presentSprite;
        memory.sprite = memorySprite;
        present.GetComponent<WorldLayerObject>().mode = WorldLayerMode.Present;
        memory.GetComponent<WorldLayerObject>().mode = WorldLayerMode.Memory;
        SetBounds(roomBounds, present.bounds);

        var music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Prologue/BGM_Prologue_ArMorDu_loop_v1.ogg");
        Undo.RecordObject(level, "Continue Prologue music in Home");
        level.musicClip = music;
        level.musicVolume = 0.26f;
        level.audioFadeSeconds = 1.5f;

        DestroyIfFound("Ladder_Broken");
        DestroyIfFound("PickupPrompt");
        var flowText = AssetDatabase.LoadAssetAtPath<LanternFlowText>("Assets/_Project/Data/Storyboard/LanternFlowText.asset");
        var broken = GetOrCreate("BrokenLadderHintZone");
        broken.transform.position = new Vector3(2.8f, -2.9f, 0f);
        var brokenCollider = Ensure<BoxCollider2D>(broken);
        brokenCollider.isTrigger = true;
        brokenCollider.size = new Vector2(1.6f, 1.3f);
        var brokenHint = Ensure<BrokenLadderHintZone>(broken);
        SetSerialized(brokenHint, "flowText", flowText);
        Ensure<WorldLayerObject>(broken).mode = WorldLayerMode.Present;

        var lanternSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Lantern_v4.png");
        var oldPickupArt = pickup.GetComponent<SpriteRenderer>();
        Undo.RecordObject(oldPickupArt, "Hide old pickup lantern art");
        oldPickupArt.enabled = false;
        var pickupArt = GetOrCreateChild(pickup.transform, "LanternArt");
        pickupArt.transform.localPosition = Vector3.zero;
        pickupArt.transform.localScale = Vector3.one * (0.04f / pickup.transform.lossyScale.x);
        var pickupRenderer = Ensure<SpriteRenderer>(pickupArt);
        pickupRenderer.sprite = lanternSprite;
        pickupRenderer.sharedMaterial = present.sharedMaterial;
        pickupRenderer.sortingOrder = 7;

        var atticArtObject = GetOrCreateChild(attic.transform, "Attic_Art");
        atticArtObject.transform.position = new Vector3(6f, 5.4f, 0f);
        atticArtObject.transform.localScale = Vector3.one * 0.45f;
        var atticRenderer = Ensure<SpriteRenderer>(atticArtObject);
        atticRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Attic_PerspectiveRef_v3.png");
        atticRenderer.sharedMaterial = present.sharedMaterial;
        atticRenderer.sortingOrder = -90;
        foreach (var renderer in attic.GetComponentsInChildren<SpriteRenderer>(true))
            if (renderer != atticRenderer) { Undo.RecordObject(renderer, "Hide attic graybox render"); renderer.enabled = false; }
        int groundLayer = LayerMask.NameToLayer("Ground");
        foreach (var collider in attic.GetComponentsInChildren<Collider2D>(true))
            if (collider.name.StartsWith("Attic_Floor_"))
            {
                Undo.RecordObject(collider.gameObject, "Make attic floor count as ground");
                collider.gameObject.layer = groundLayer;
            }
        var atticBoundsObject = GetOrCreate("AtticCameraBounds");
        var atticBounds = Ensure<PolygonCollider2D>(atticBoundsObject);
        SetBounds(atticBounds, atticRenderer.bounds);
        var cameraZone = attic.GetComponent<AtticCameraZone>();
        Undo.RecordObject(cameraZone, "Set separate attic camera confiner");
        cameraZone.roomBounds = roomBounds;
        cameraZone.atticBounds = atticBounds;
        cameraZone.atticArtwork = atticRenderer;
        cameraZone.atticOrthographicSize = 3.4f;
        cameraZone.atticTargetOffset = new Vector3(0f, 3.25f, 0f);
        var landing = GetOrCreateChild(attic.transform, "AtticLanding");
        landing.transform.position = new Vector3(4.2f, 3.25f, 0f);

        for (int i = whole.transform.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(whole.transform.GetChild(i).gameObject);
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(whole);
        whole.transform.position = new Vector3(2.8f, -0.45f, 0f);
        var ladderArt = GetOrCreateChild(whole.transform, "LadderArt");
        ladderArt.transform.localPosition = Vector3.zero;
        ladderArt.transform.localScale = Vector3.one * (5.8f / 42f);
        var ladderRenderer = Ensure<SpriteRenderer>(ladderArt);
        ladderRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Ladder_Whole_v5.png");
        ladderRenderer.sharedMaterial = present.sharedMaterial;
        ladderRenderer.sortingOrder = 5;
        var ladderCollider = whole.GetComponent<BoxCollider2D>();
        Undo.RecordObject(ladderCollider, "Use ladder base as transition volume");
        ladderCollider.isTrigger = true;
        ladderCollider.offset = new Vector2(0f, -2.5f);
        ladderCollider.size = new Vector2(1.65f, 1.25f);
        whole.GetComponent<WorldLayerObject>().mode = WorldLayerMode.Memory;

        var fadeObject = FindSceneObject("AtticTransitionCanvas");
        if (!fadeObject)
        {
            fadeObject = new GameObject("AtticTransitionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(fadeObject, "Create attic transition canvas");
        }
        var canvas = Ensure<Canvas>(fadeObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        var scaler = Ensure<CanvasScaler>(fadeObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var fade = Ensure<CanvasGroup>(fadeObject);
        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        var black = fadeObject.transform.Find("FullBlack")?.gameObject;
        if (!black)
        {
            black = new GameObject("FullBlack", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(black, "Create transition black panel");
            black.transform.SetParent(fadeObject.transform, false);
        }
        var blackRect = black.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = blackRect.offsetMax = Vector2.zero;
        var blackImage = Ensure<Image>(black);
        blackImage.color = Color.black;
        blackImage.raycastTarget = true;
        var transition = Ensure<LadderAtticTransition>(whole);
        SetSerialized(transition, "atticLanding", landing.transform);
        SetSerialized(transition, "atticCamera", cameraZone);
        SetSerialized(transition, "fullScreenFade", fade);

        endTrigger.transform.position = new Vector3(7.3f, 3.25f, 0f);
        var endCollider = endTrigger.GetComponent<BoxCollider2D>();
        Undo.RecordObject(endCollider, "Move window trigger to attic floor");
        endCollider.size = new Vector2(1.2f, 1.35f);
        SetSerialized(endTrigger.GetComponent<EndTrigger_TEMP>(), "fade", fade);
        SaveActiveScene();
    }

    private static void UpdateEndCard()
    {
        EditorSceneManager.OpenScene(End, OpenSceneMode.Single);
        var root = FindSceneObject("EndCardButtons")?.GetComponent<RectTransform>();
        var back = FindSceneObject("BackToMenuButton")?.GetComponent<RectTransform>();
        var quit = FindSceneObject("QuitButton")?.GetComponent<RectTransform>();
        var inputHint = FindSceneObject("Input Hint")?.GetComponent<TMP_Text>();
        var controller = FindFirst<EndCardController>();
        if (!root || !back || !quit || !controller) throw new InvalidOperationException("EndCard button hierarchy is missing.");
        if (root.Find("SelectionIcon") && root.Find("SelectionUnderline"))
        {
            SetSerialized(controller, "advanceHint", inputHint);
            SaveActiveScene();
            return;
        }
        Undo.RecordObject(root, "Scale complete EndCard controls");
        root.sizeDelta = new Vector2(390f, 112f);
        SetButton(back, new Vector2(315f, 51f), new Vector2(0f, 18f));
        SetButton(quit, new Vector2(315f, 51f), new Vector2(0f, -27f));
        SetSerialized(controller, "advanceHint", inputHint);
        SaveActiveScene();
    }

    private static void SetButton(RectTransform rect, Vector2 size, Vector2 position)
    {
        Undo.RecordObject(rect, "Resize EndCard button");
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var label = rect.GetComponentInChildren<TMP_Text>(true);
        if (!label) return;
        Undo.RecordObject(label, "Resize EndCard button label");
        label.fontSize = 24f;
        Undo.RecordObject(label.rectTransform, "Resize EndCard label padding");
        label.rectTransform.sizeDelta = new Vector2(-18f, -9f);
    }

    private static void SetBounds(PolygonCollider2D collider, Bounds world)
    {
        Undo.RecordObject(collider, "Confine camera to selected art");
        collider.isTrigger = true;
        collider.pathCount = 1;
        collider.SetPath(0, new[]
        {
            Local(collider, world.min.x, world.min.y), Local(collider, world.max.x, world.min.y),
            Local(collider, world.max.x, world.max.y), Local(collider, world.min.x, world.max.y)
        });
    }

    private static Vector2 Local(Collider2D collider, float x, float y)
    {
        Vector3 point = collider.transform.InverseTransformPoint(new Vector3(x, y, 0f));
        return new Vector2(point.x, point.y);
    }

    private static void SetSerialized(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(name);
        if (property == null) throw new InvalidOperationException(target.name + " has no field " + name);
        property.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string name, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(name).floatValue = value;
        so.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string name, Vector3 value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(name).vector3Value = value;
        so.ApplyModifiedProperties();
    }

    private static GameObject GetOrCreate(string name)
    {
        var existing = FindSceneObject(name);
        if (existing) return existing;
        var created = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(created, "Create " + name);
        return created;
    }

    private static T Ensure<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component ? component : go.AddComponent<T>();
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child) return child.gameObject;
        var created = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(created, "Create " + name);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static GameObject FindSceneObject(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindDeep(root.transform, name);
            if (found) return found.gameObject;
        }
        return null;
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindDeep(child, name);
            if (found) return found;
        }
        return null;
    }

    private static void DestroyIfFound(string name)
    {
        var old = FindSceneObject(name);
        if (old) Undo.DestroyObjectImmediate(old);
    }

    private static T FindFirst<T>() where T : UnityEngine.Object
        => UnityEngine.Object.FindFirstObjectByType<T>();

    private static void SaveActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
