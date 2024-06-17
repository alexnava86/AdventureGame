using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

public class BuildScript
{
    public string[] buildScenes;

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
        //string[] scenes = GetScenes().ToArray();
        string[] scenes = {"Assets/Scenes/AdventureWaterfall1_TEST.unity"}; //TEST
        string buildDir = "/project/Builds/Windows";
        string path = buildDir + "/adventuregame-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".exe";
        
        Debug.Log("Build directory: " + buildDir);
        Debug.Log("Build path: " + path);

        if (!Directory.Exists(buildDir))
        {
            Debug.Log("Build directory does not exist. Creating directory...");
            Directory.CreateDirectory(buildDir);
        }
        else
        {
            Debug.Log("Build directory already exists.");
        }
        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Windows Build completed successfully. Build path: " + path);
    }

    public static void BuildMacOS()
    {
        string[] scenes = GetScenes().ToArray();
        string path = "/project/Builds/MacOS/adventuregame-" + DateTime.Now.ToShortDateString() + ".app";
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
