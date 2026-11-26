using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class SmokeBuild
{
    private const string OutputPath = "C:/Users/crew3/AppData/Local/Temp/opencode/mp-build/SummonSupport.exe";

    public static void BuildAndExit()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        BuildReport report = BuildPipeline.BuildPlayer(scenes, OutputPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
