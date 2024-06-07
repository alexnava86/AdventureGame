using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

public class BuildScript
{
    
    public static void BuildAll()
    {
        BuildWindows();
        BuildMacOS();
        BuildWebGL();
    }
    public static List<string> GetScenes()
    {
        List<string> scenes = new List<string>();

        string folderPath = "Assets/Scenes";
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { folderPath });

        foreach (string guid in guids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            scenes.Add(sceneName);
        }
        return scenes;
    }

    public static void BuildWindows()
    {
        string[] scenes = GetScenes().ToArray();
        string path = "Builds/Windows/adventuregame-" + DateTime.Now.ToShortDateString() + ".exe";
        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Windows Build completed successfully.");
    }

    public static void BuildMacOS()
    {
        string[] scenes = GetScenes().ToArray();
        string path = "Builds/MacOS/adventuregame-" + DateTime.Now.ToShortDateString() + ".app";
        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("macOS Build completed successfully.");
    }

    public static void BuildWebGL()
    {
        string[] scenes = GetScenes().ToArray();
        string path = "Builds/WebGL";
        string zipPath = "Builds/WebGL/adventuregame-" + DateTime.Now.ToShortDateString() + ".zip";
        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.WebGL, BuildOptions.None);
        ZipFile.CreateFromDirectory(path, zipPath);
        Debug.Log("WebGL Build completed successfully.");
    }
}
