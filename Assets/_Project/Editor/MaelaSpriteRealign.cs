// Re-aligns Maela sprite children after layer bounds change in Maela.psb.
// Data: Assets/_Project/Art/Characters/Maela/maela_realign.json  (written by Claude from old/new PSB bounds)
//   {"ppu":200,"layers":[{"name":"Boot_L","dx_px":2.0,"dy_px":-1.5}, ...]}   dx right, dy DOWN, in PSB pixels (centre shift)
// Menu: Tools > Lantern Keeper > Realign Maela Sprites. Applies to the Maela prefab (and any scene instance inherits it).
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MaelaSpriteRealign
{
    const string Json = "Assets/_Project/Art/Characters/Maela/maela_realign.json";
    const string Prefab = "Assets/_Project/Prefabs/Maela.prefab";

    [System.Serializable] class Entry { public string name; public float dx_px; public float dy_px; }
    [System.Serializable] class Data { public float ppu = 200; public Entry[] layers; public string applied_for; }

    [MenuItem("Tools/Lantern Keeper/Realign Maela Sprites")]
    public static void Run()
    {
        if (!File.Exists(Json)) { Debug.LogError("Realign: missing " + Json); return; }
        var d = JsonUtility.FromJson<Data>(File.ReadAllText(Json));
        var root = PrefabUtility.LoadPrefabContents(Prefab);
        int n = 0;
        foreach (var e in d.layers)
        {
            Transform t = null;
            foreach (var c in root.GetComponentsInChildren<Transform>(true))
                if (c.name == e.name + "_Sprite") { t = c; break; }
            if (t == null) { Debug.LogWarning("Realign: no child " + e.name + "_Sprite"); continue; }
            var before = t.localPosition;
            t.localPosition = before + new Vector3(e.dx_px / d.ppu, -e.dy_px / d.ppu, 0);
            Debug.Log($"Realign {t.name}: {before} -> {t.localPosition}");
            n++;
        }
        PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"Realign done: {n} sprites moved. Run it ONCE per psb version.");
    }
}
