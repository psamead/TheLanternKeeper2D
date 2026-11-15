using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Reproducible release exports; call from a clean, saved Editor outside Play Mode.</summary>
public static class DemoReleaseBuilder
{
    const string Root = "Builds/FinalDemo";
    static readonly string[] Scenes = { "00_StartMenu", "00b_Prologue", "Core_Persistent", "01_Home", "03_EndCard" };

    [MenuItem("Tools/Lantern Keeper/Release/Build Windows x64")]
    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows/TheLanternKeeper2D.exe");

    [MenuItem("Tools/Lantern Keeper/Release/Build Web")]
    public static void BuildWeb() => Build(BuildTarget.WebGL, "Web");

    public static void ConfigureWeb()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.template = "PROJECT:Lantern";
        PlayerSettings.defaultWebScreenWidth = 1920;
        PlayerSettings.defaultWebScreenHeight = 1080;
        AssetDatabase.SaveAssets();
    }

    static void Build(BuildTarget target, string relativeOutput)
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before building.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save scene edits before building.");
        Directory.CreateDirectory(Root);
        string[] scenes = Scenes.Select(s => "Assets/_Project/Scenes/" + s + ".unity").ToArray();
        string output = Path.Combine(Root, relativeOutput);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = scenes, locationPathName = output, target = target, options = BuildOptions.None });
        var summary = report.summary;
        string source = File.Exists(Root + "/source-commit.txt") ? File.ReadAllText(Root + "/source-commit.txt").Trim() : "unrecorded";
        string log = "Unity=" + Application.unityVersion + "\nSource=" + source + "\nTarget=" + target +
            "\nOptions=None (non Development)\nBackend=" + PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target))) +
            "\nScenes=" + string.Join(",", scenes) + "\nResult=" + summary.result +
            "\nBytes=" + summary.totalSize + "\nDuration=" + summary.totalTime +
            "\nErrors=" + summary.totalErrors + "\nWarnings=" + summary.totalWarnings + "\n";
        foreach (var step in report.steps)
            foreach (var message in step.messages)
                if (message.type == LogType.Error || message.type == LogType.Exception || message.type == LogType.Warning)
                    log += message.type + ": " + message.content + "\n";
        File.WriteAllText(Root + "/BuildReport_" + target + ".txt", log);
        if (summary.result != BuildResult.Succeeded) throw new Exception("Release build failed: " + target);
        Debug.Log("Release build succeeded: " + output);
    }
}
