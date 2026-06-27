// =============================================================================
// BuildScript.cs   —   Assets/Editor/
//
// Entry points Unity calls in batch mode (from CI) to build the game for each
// platform. Invoked via the command line like:
//
//   Unity -quit -batchmode -nographics -projectPath . \
//         -executeMethod BuildScript.BuildWindows
//
// GameCI's unity-builder calls these for you when you set buildMethod in the
// workflow. Each method writes its output under Builds/<Platform>/.
//
// The scene list is read from the Build Settings (EditorBuildSettings), so make
// sure your scenes are added there (File ▸ Build Settings ▸ Add Open Scenes).
// =============================================================================

using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    // Where builds are written (relative to the project root).
    private const string BuildRoot = "Builds";

    // -------------------------------------------------------------------------
    // Public entry points — one per platform target
    // -------------------------------------------------------------------------

    public static void BuildWindows()
    {
        string v = GetBuildVersion();
        Build(BuildTarget.StandaloneWindows64,
              $"{BuildRoot}/Windows/adventure-game-{v}.exe", v);
    }

    public static void BuildMac()
    {
        // macOS produces a .app bundle (a folder). The path ends in .app.
        string v = GetBuildVersion();
        Build(BuildTarget.StandaloneOSX,
              $"{BuildRoot}/Mac/adventure-game-{v}.app", v);
    }

    public static void BuildWebGL()
    {
        // WebGL outputs a folder of web files (index.html, Build/, etc.).
        string v = GetBuildVersion();
        Build(BuildTarget.WebGL,
              $"{BuildRoot}/WebGL/adventure-game-{v}", v);
    }

    // Optional: build all three in one invocation (useful for local testing).
    public static void BuildAll()
    {
        BuildWindows();
        BuildMac();
        BuildWebGL();
    }

    // -------------------------------------------------------------------------
    // Core build routine
    // -------------------------------------------------------------------------

    private static void Build(BuildTarget target, string locationPathName, string version)
    {
        // Stamp the version into the build itself so it shows in the player /
        // executable metadata, not just the file name.
        if (!string.IsNullOrEmpty(version))
            PlayerSettings.bundleVersion = version;

        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No enabled scenes in Build Settings. " +
                           "Add scenes via File ▸ Build Settings before building.");
            EditorApplication.Exit(1);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = locationPathName,
            target           = target,
            options          = BuildOptions.None
        };

        Debug.Log($"[BuildScript] Building {target} → {locationPathName} " +
                  $"with {scenes.Length} scene(s).");

        var report  = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] {target} build SUCCEEDED: " +
                      $"{summary.totalSize} bytes in {summary.totalTime}.");
            // In batch mode, exit 0 so CI marks the step green.
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[BuildScript] {target} build FAILED: {summary.result}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
                                  .Where(s => s.enabled)
                                  .Select(s => s.path)
                                  .ToArray();
    }

    /// <summary>
    /// Reads the version CI wants this build stamped with. Checks, in order:
    ///   1. the "-buildVersion X.Y.Z" command-line argument, then
    ///   2. the BUILD_VERSION environment variable (fallback if GameCI doesn't
    ///      forward the command-line arg), then
    ///   3. the project's current bundleVersion (for local builds with neither).
    /// </summary>
    private static string GetBuildVersion()
    {
        // 1) command-line argument
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-buildVersion")
                return args[i + 1];

        // 2) environment variable
        string env = System.Environment.GetEnvironmentVariable("BUILD_VERSION");
        if (!string.IsNullOrEmpty(env))
            return env;

        // 3) fallback to whatever the project already has
        return PlayerSettings.bundleVersion;
    }
}
