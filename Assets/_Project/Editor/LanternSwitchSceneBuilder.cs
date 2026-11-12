using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Repeatable editor construction for the LanternSwitch Home graybox.</summary>
public static class LanternSwitchSceneBuilder
{
    private const string CorePath = "Assets/_Project/Scenes/Core_Persistent.unity";
    private const string HomePath = "Assets/_Project/Scenes/01_Home.unity";
    private static Sprite blockSprite;
    private static Material blockMaterial;

    public static void BuildCore()
    {
        EditorSceneManager.OpenScene(CorePath, OpenSceneMode.Single);
        var stateObject = GameObject.Find("GameState");
        var playerObject = GameObject.Find("Player");
        var lightObject = GameObject.Find("MaelaPreview_GlobalLight");
        if (!stateObject || !playerObject || !lightObject)
            throw new System.InvalidOperationException("Core_Persistent is missing GameState, Player, or Global Light.");
        var switcher = stateObject.GetComponent<LanternSwitch>() ?? Undo.AddComponent<LanternSwitch>(stateObject);
        switcher.player = playerObject.GetComponent<PlayerController>();
        switcher.maelaAudio = playerObject.GetComponent<MaelaAudio>();
        switcher.globalLight = lightObject.GetComponent<Light2D>();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    public static void BuildHome()
    {
        EditorSceneManager.OpenScene(HomePath, OpenSceneMode.Single);
        blockSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        blockMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        if (!blockSprite) throw new System.InvalidOperationException("Unity built-in UISprite is unavailable.");

        var background = GameObject.Find("Background");
        var levelInfo = GameObject.Find("LevelInfo")?.GetComponent<LevelInfo>();
        var bounds = GameObject.Find("CameraBounds")?.GetComponent<PolygonCollider2D>();
        if (!background || !levelInfo || !bounds) throw new System.InvalidOperationException("01_Home is missing Background, LevelInfo, or CameraBounds.");
        var presentBackground = background.GetComponent<WorldLayerObject>() ?? Undo.AddComponent<WorldLayerObject>(background);
        presentBackground.mode = WorldLayerMode.Present;
        DestroyIfFound("Home_Memory_bg");
        var memoryBackground = new GameObject("Home_Memory_bg");
        Undo.RegisterCreatedObjectUndo(memoryBackground, "Add memory background");
        memoryBackground.transform.SetPositionAndRotation(background.transform.position, background.transform.rotation);
        memoryBackground.transform.localScale = background.transform.localScale;
        var presentRenderer = background.GetComponent<SpriteRenderer>();
        var memoryRenderer = memoryBackground.AddComponent<SpriteRenderer>();
        memoryRenderer.sprite = presentRenderer.sprite;
        memoryRenderer.sortingLayerID = presentRenderer.sortingLayerID;
        memoryRenderer.sortingOrder = presentRenderer.sortingOrder + 1;
        memoryRenderer.color = new Color32(0xF2, 0xC4, 0x8A, 0xFF);
        var memoryLayer = memoryBackground.AddComponent<WorldLayerObject>();
        memoryLayer.mode = WorldLayerMode.Memory;

        DestroyIfFound("LanternSwitch_Graybox");
        DestroyIfFound("Ladder_Broken");
        DestroyIfFound("Ladder_Whole");
        DestroyIfFound("Attic_Greybox");
        DestroyIfFound("Ladder_Hint");
        DestroyIfFound("EndTrigger_TEMP");
        DestroyIfFound("EndTrigger_AtticWindow");

        CreateLadder("Ladder_Broken", new Vector2(2.8f, -2.35f), 2.0f, WorldLayerMode.Present,
            new Color32(0xA0, 0x8D, 0x75, 0xFF), false);
        CreateLadder("Ladder_Whole", new Vector2(2.8f, 0.25f), 6.5f, WorldLayerMode.Memory,
            new Color32(0xC5, 0x91, 0x56, 0xFF), true);

        CreateAttic();
        CreateWindowTrigger();
        bounds.pathCount = 1;
        // The polygon includes full camera-frustum padding at both the spawn and attic ends.
        bounds.SetPath(0, new[] { new Vector2(-18.1f, -10f), new Vector2(21.9f, -10f), new Vector2(21.9f, 18f), new Vector2(-18.1f, 18f) });
        levelInfo.globalLightColor = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
        levelInfo.globalLightIntensity = 0.8f;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static void CreateLadder(string name, Vector2 center, float height, WorldLayerMode mode, Color color, bool whole)
    {
        var root = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(root, "Create ladder graybox");
        root.transform.position = center;
        var trigger = root.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.0f, height);
        var zone = root.AddComponent<LadderClimbZone>();
        zone.canClimb = whole;
        zone.showMemoryHintOnFirstVisit = whole;
        var layer = root.AddComponent<WorldLayerObject>();
        layer.mode = mode;
        float railHeight = whole ? height : height * 0.86f;
        MakeSprite(root.transform, "Rail_L", new Vector2(-0.38f, 0f), new Vector2(0.22f, railHeight), color, 5);
        MakeSprite(root.transform, "Rail_R", new Vector2(0.38f, 0f), new Vector2(0.22f, railHeight), color, 5);
        int rungs = whole ? 13 : 4;
        for (int i = 0; i < rungs; i++)
        {
            float t = (i + 0.5f) / rungs;
            float y = Mathf.Lerp(-railHeight * 0.5f, railHeight * 0.5f, t);
            MakeSprite(root.transform, "Rung_" + (i + 1), new Vector2(0f, y), new Vector2(0.84f, 0.14f), Color.Lerp(color, Color.black, 0.12f), 6);
        }
        if (!whole)
        {
            MakeSprite(root.transform, "BrokenTop_L", new Vector2(-0.34f, railHeight * 0.5f), new Vector2(0.16f, 0.22f), new Color32(0x4E, 0x49, 0x3F, 0xFF), 6);
            MakeSprite(root.transform, "BrokenTop_R", new Vector2(0.34f, railHeight * 0.5f - 0.18f), new Vector2(0.16f, 0.22f), new Color32(0x4E, 0x49, 0x3F, 0xFF), 6);
        }
    }

    private static void CreateAttic()
    {
        var root = new GameObject("Attic_Greybox");
        Undo.RegisterCreatedObjectUndo(root, "Create attic graybox");
        MakeSolid(root.transform, "Attic_BackWall", new Vector2(6f, 5.95f), new Vector2(13.5f, 8.0f), new Color32(0x6A, 0x62, 0x5A, 0xFF), 1, false);
        var floorColor = new Color32(0x8A, 0x77, 0x61, 0xFF);
        // Leave a ladder-width opening so Maela can climb through the attic floor.
        MakeSolid(root.transform, "Attic_Floor_Left", new Vector2(2.0f, 2.55f), new Vector2(0.4f, 0.34f), floorColor, 4, true);
        MakeSolid(root.transform, "Attic_Floor_Right", new Vector2(6.8f, 2.55f), new Vector2(6.8f, 0.34f), floorColor, 4, true);
        MakeSolid(root.transform, "Attic_WallL", new Vector2(1.9f, 5.35f), new Vector2(0.34f, 5.7f), new Color32(0x77, 0x70, 0x67, 0xFF), 3, true);
        MakeSolid(root.transform, "Attic_WallR", new Vector2(10.1f, 5.35f), new Vector2(0.34f, 5.7f), new Color32(0x77, 0x70, 0x67, 0xFF), 3, true);
        MakeSolid(root.transform, "Attic_Roof_L", new Vector2(3.85f, 8.35f), new Vector2(4.5f, 0.32f), new Color32(0x56, 0x51, 0x4C, 0xFF), 4, true, -25f);
        MakeSolid(root.transform, "Attic_Roof_R", new Vector2(8.15f, 8.35f), new Vector2(4.5f, 0.32f), new Color32(0x56, 0x51, 0x4C, 0xFF), 4, true, 25f);
        MakeSprite(root.transform, "Attic_WindowFrame", new Vector2(9.75f, 5.75f), new Vector2(0.22f, 2.0f), new Color32(0xBA, 0xA1, 0x7B, 0xFF), 7);
        MakeSprite(root.transform, "Attic_WindowGlass", new Vector2(9.62f, 5.75f), new Vector2(0.09f, 1.55f), new Color32(0x20, 0x31, 0x46, 0xFF), 8);
        MakeSprite(root.transform, "Lighthouse_Silhouette", new Vector2(9.50f, 5.82f), new Vector2(0.14f, 0.76f), new Color32(0x0B, 0x12, 0x20, 0xFF), 9);
        MakeSprite(root.transform, "Lighthouse_Lantern", new Vector2(9.50f, 6.24f), new Vector2(0.27f, 0.13f), new Color32(0x0B, 0x12, 0x20, 0xFF), 9);
        MakeSprite(root.transform, "Attic_WindowSill", new Vector2(9.38f, 4.95f), new Vector2(0.55f, 0.16f), new Color32(0x9B, 0x7B, 0x54, 0xFF), 9);
        var cameraZone = root.AddComponent<BoxCollider2D>();
        cameraZone.isTrigger = true;
        cameraZone.offset = new Vector2(6f, 5.35f);
        cameraZone.size = new Vector2(8.2f, 5.8f);
        var cameraFraming = root.AddComponent<AtticCameraZone>();
        cameraFraming.atticOrthographicSize = 3.4f;
        cameraFraming.atticTargetOffset = new Vector3(0f, 3.25f, 0f);
        var layer = root.AddComponent<WorldLayerObject>();
        layer.mode = WorldLayerMode.Both;
    }

    private static void CreateWindowTrigger()
    {
        var go = new GameObject("EndTrigger_AtticWindow");
        Undo.RegisterCreatedObjectUndo(go, "Move end trigger to attic window");
        go.transform.position = new Vector3(8.9f, 5.45f, 0f);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.2f, 2.3f);
        go.AddComponent<EndTrigger_TEMP>();
        go.AddComponent<WorldLayerObject>().mode = WorldLayerMode.Both;
    }

    private static void MakeSolid(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder, bool collision, float rotation = 0f)
    {
        var go = MakeSprite(parent, name, position, size, color, sortingOrder);
        go.transform.localEulerAngles = new Vector3(0f, 0f, rotation);
        if (collision)
        {
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(blockSprite.bounds.size.x, blockSprite.bounds.size.y);
        }
    }

    private static GameObject MakeSprite(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        Undo.RegisterCreatedObjectUndo(go, "Create graybox block");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        Vector3 spriteSize = blockSprite.bounds.size;
        go.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = blockSprite;
        if (blockMaterial) renderer.sharedMaterial = blockMaterial;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    private static void DestroyIfFound(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == name) Undo.DestroyObjectImmediate(root);
    }
}
