// Verifies that every sprite in Maela.psb has rect size == its PSB layer size (Claude, 2026-11-08).
// Data: Assets/_Project/Art/Characters/Maela/maela_expected_rects.json (written by patch_psb_meta.py from the PSB).
// Menu: Tools > Lantern Keeper > Check Maela Sprite Rects. Logs one line per sprite; ends with PASS or FAIL.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MaelaSpriteRectCheck
{
    const string Psb = "Assets/_Project/Art/Characters/Maela/Maela.psb";
    const string Json = "Assets/_Project/Art/Characters/Maela/maela_expected_rects.json";

    [System.Serializable] class S { public string name; public int w, h, posX, posY; }
    [System.Serializable] class D { public string psb; public int canvasH; public S[] sprites; }

    [MenuItem("Tools/Lantern Keeper/Check Maela Sprite Rects")]
    public static void Run()
    {
        if (!File.Exists(Json)) { Debug.LogError("RectCheck: missing " + Json); return; }
        var d = JsonUtility.FromJson<D>(File.ReadAllText(Json));
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Psb).OfType<Sprite>().ToDictionary(s => s.name);
        int bad = 0;
        foreach (var e in d.sprites)
        {
            if (!sprites.TryGetValue(e.name, out var s)) { Debug.LogError($"RectCheck MISSING {e.name}"); bad++; continue; }
            var r = s.rect;
            bool ok = Mathf.RoundToInt(r.width) == e.w && Mathf.RoundToInt(r.height) == e.h;
            string line = $"RectCheck {(ok ? "ok  " : "FAIL")} {e.name}: sprite.rect {r.width}x{r.height} @({r.x},{r.y}) / PSB layer {e.w}x{e.h}";
            if (ok) Debug.Log(line); else { Debug.LogError(line); bad++; }
        }
        var refs = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Maela.prefab")
            .GetComponentsInChildren<SpriteRenderer>(true).Where(x => x.sprite == null).Select(x => x.name).ToArray();
        if (refs.Length > 0) { Debug.LogError("RectCheck prefab missing sprite: " + string.Join(", ", refs)); bad += refs.Length; }
        Debug.Log(bad == 0 ? "RectCheck PASS" : $"RectCheck FAIL ({bad})");
    }
}
