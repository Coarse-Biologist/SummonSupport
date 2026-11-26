using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class SmokeBuild
{
    public static void BuildAndExit()
    {
        string folder = "C:/Users/crew3/AppData/Local/Temp/opencode/mp-build-" + System.DateTime.Now.ToString("yyyyMMdd-HHmm");
        System.IO.Directory.CreateDirectory(folder);
        string outputPath = folder + "/SummonSupport.exe";
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        UnityEngine.Debug.Log("SMOKE BUILD OUTPUT: " + outputPath);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
