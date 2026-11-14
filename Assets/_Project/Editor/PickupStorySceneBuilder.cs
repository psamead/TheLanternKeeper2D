using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Focused authoring for the pickup gate, supported landings and narrative audio cues.</summary>
public static class PickupStorySceneBuilder
{
    [MenuItem("Tools/Lantern Keeper/Apply Pickup Story Attic Safety")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before applying.");
        var home = EditorSceneManager.OpenScene("Assets/_Project/Scenes/01_Home.unity", OpenSceneMode.Single);
        var all = home.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
        Transform Find(string name) => all.Single(t => t.name == name);
        var anchor = all.FirstOrDefault(t => t.name == "ReturnAfterStoryA");
        if (!anchor) anchor = new GameObject("ReturnAfterStoryA").transform;
        anchor.position = new Vector3(-4.7f, -3.2f, 0f);
        var pickup = Find("Lantern_Pickup").GetComponent<LanternPickupInteractable>();
        var serialized = new SerializedObject(pickup);
        serialized.FindProperty("returnAfterStoryA").objectReferenceValue = anchor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Find("AtticLanding").position = new Vector3(4.7f, 2.725f, 0f);
        var floor = Find("Attic_Floor_Right").GetComponent<BoxCollider2D>();
        // Existing artwork is untouched. Extend one physical floor across the former ladder hole.
        floor.transform.position = new Vector3(6f, 2.55f, 0f);
        floor.size = new Vector2(8.4f / Mathf.Abs(floor.transform.lossyScale.x),
            0.35f / Mathf.Abs(floor.transform.lossyScale.y));
        Find("Attic_Floor_Left").GetComponent<Collider2D>().enabled = false;
        foreach (string name in new[] { "Attic_Floor_Left", "Attic_Floor_Right", "Attic_WallL", "Attic_WallR", "Attic_Roof_L", "Attic_Roof_R" })
        {
            var t = Find(name);
            t.gameObject.layer = LayerMask.NameToLayer("Ground");
            var world = t.GetComponent<WorldLayerObject>();
            if (world) world.mode = WorldLayerMode.Both;
        }
        var level = home.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LevelInfo>(true)).Single();
        var a = AssetDatabase.LoadAssetAtPath<StoryboardData>("Assets/_Project/Data/Storyboard/SB_LanternOrigin_A.asset");
        var b = AssetDatabase.LoadAssetAtPath<StoryboardData>("Assets/_Project/Data/Storyboard/SB_Ending_B.asset");
        var waves = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Storyboard/AMB_Story_Waves_loop_v1.ogg");
        var wind = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Storyboard/AMB_Story_Wind_loop_v1.ogg");
        if (!waves || !wind) throw new InvalidOperationException("Import both independent story stems first.");
        for (int i = 0; i < a.frames.Length; i++)
            a.frames[i].audioCue = new StoryboardData.AudioCue {
                enabled = true, musicVolume = i < 2 ? 0.6f : 0.38f,
                musicStartVolume = i == 0 ? 0.25f : -1f,
                ambience = i < 2 ? level.ambienceClip : i == 2 ? waves : null,
                ambienceVolume = i < 2 ? 0.18f : i == 2 ? 0.55f : 0f, fadeSeconds = 1f };
        foreach (var frame in b.frames)
            frame.audioCue = new StoryboardData.AudioCue { enabled = true, musicVolume = 0.6f,
                ambience = wind, ambienceVolume = 0.4f, fadeSeconds = 1f };
        EditorUtility.SetDirty(a); EditorUtility.SetDirty(b);
        AssetDatabase.SaveAssetIfDirty(a); AssetDatabase.SaveAssetIfDirty(b);
        EditorSceneManager.MarkSceneDirty(home);
        EditorSceneManager.SaveScene(home);
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_StartMenu.unity", OpenSceneMode.Single);
    }
}
