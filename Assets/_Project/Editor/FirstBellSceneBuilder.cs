using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Focused, repeatable authoring of the first-bell zones and fixed prompts.</summary>
public static class FirstBellSceneBuilder
{
    [MenuItem("Tools/Lantern Keeper/Apply First Bell Exploration")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before applying.");
        var core = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Core_Persistent.unity", OpenSceneMode.Single);
        var template = core.GetRootGameObjects().Single(g => g.name == "LanternProgressHint").GetComponentInChildren<TMP_Text>(true);
        var home = EditorSceneManager.OpenScene("Assets/_Project/Scenes/01_Home.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(home);
        var roots = home.GetRootGameObjects();
        var holder = roots.FirstOrDefault(g => g.name == "FirstBellExploration") ?? new GameObject("FirstBellExploration");
        var hints = holder.GetComponent<FirstBellExplorationHints>();
        if (!hints) hints = holder.AddComponent<FirstBellExplorationHints>();
        var flow = AssetDatabase.LoadAssetAtPath<LanternFlowText>("Assets/_Project/Data/Storyboard/LanternFlowText.asset");
        var broken = MakeText(holder.transform, template, "BrokenLadderFixedHint", new Vector3(2.8f, -0.65f, 0f), flow.brokenLadderHint);
        var press = MakeText(holder.transform, template, "PressEFixedHint", new Vector3(-3f, -0.65f, 0f), flow.raiseHint);
        var serialized = new SerializedObject(hints);
        serialized.FindProperty("flowText").objectReferenceValue = flow;
        serialized.FindProperty("brokenLadder").objectReferenceValue = broken;
        serialized.FindProperty("pressE").objectReferenceValue = press;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var discovery = roots.FirstOrDefault(g => g.name == "LanternDiscoveryZone") ?? new GameObject("LanternDiscoveryZone");
        ConfigureZone(discovery, new Vector3(-3f, -2.9f, 0), new Vector2(1.2f, 1.3f), true);
        var ladder = roots.Single(g => g.name == "BrokenLadderHintZone");
        ConfigureZone(ladder, new Vector3(2.8f, -2.9f, 0), new Vector2(1.6f, 1.3f), false);
        EditorSceneManager.MarkSceneDirty(home);
        EditorSceneManager.SaveScene(home);
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_StartMenu.unity", OpenSceneMode.Single);
    }

    private static TMP_Text MakeText(Transform parent, TMP_Text template, string name, Vector3 position, string message)
    {
        var anchor = parent.Find(name + "Anchor");
        if (!anchor)
        {
            anchor = new GameObject(name + "Anchor").transform;
            anchor.SetParent(parent, false);
        }
        anchor.position = position;
        var existing = anchor.Find(name);
        if (!existing) existing = parent.Find(name);
        var text = existing ? existing.GetComponent<TMP_Text>() : UnityEngine.Object.Instantiate(template, anchor);
        text.transform.SetParent(anchor, false);
        text.name = name;
        text.rectTransform.anchoredPosition3D = Vector3.zero;
        text.transform.localPosition = Vector3.zero;
        text.text = message;
        text.gameObject.SetActive(false);
        return text;
    }

    private static void ConfigureZone(GameObject root, Vector3 position, Vector2 size, bool discovery)
    {
        root.transform.position = position;
        var box = root.GetComponent<BoxCollider2D>();
        if (!box) box = root.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;
        var zone = root.GetComponent<BrokenLadderHintZone>();
        if (!zone) zone = root.AddComponent<BrokenLadderHintZone>();
        var serialized = new SerializedObject(zone);
        serialized.FindProperty("discovery").boolValue = discovery;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

