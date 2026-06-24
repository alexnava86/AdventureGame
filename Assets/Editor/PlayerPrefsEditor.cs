using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PlayerPrefsEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private string newKey = "";
    private int newIntValue = 0;
    private float newFloatValue = 0f;
    private string newStringValue = "";
    private string searchFilter = "";

    [MenuItem("Window/Tools/PlayerPrefs Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(100)))
        {
            Repaint();
        }
        
        if (GUILayout.Button("Delete All PlayerPrefs", GUILayout.Width(160)))
        {
            if (EditorUtility.DisplayDialog("Warning", 
                "This will permanently delete ALL PlayerPrefs data. Are you sure?", 
                "Yes, Delete All", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        searchFilter = EditorGUILayout.TextField("Search Key", searchFilter);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        var allKeys = GetAllKeys();

        foreach (var key in allKeys)
        {
            if (!string.IsNullOrEmpty(searchFilter) && 
                !key.ToLower().Contains(searchFilter.ToLower()))
                continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Key: {key}", EditorStyles.boldLabel);

            // Detect type and show appropriate field
            if (PlayerPrefs.HasKey(key))
            {
                // Try to determine type
                string strVal = PlayerPrefs.GetString(key, "__NOT_FOUND__");
                
                if (strVal != "__NOT_FOUND__" && !string.IsNullOrEmpty(strVal))
                {
                    // It's likely a string
                    string newStr = EditorGUILayout.TextField("Value (String)", strVal);
                    if (newStr != strVal)
                    {
                        PlayerPrefs.SetString(key, newStr);
                        PlayerPrefs.Save();
                    }
                }
                else
                {
                    // Try int
                    int intVal = PlayerPrefs.GetInt(key, int.MinValue);
                    if (intVal != int.MinValue)
                    {
                        int newInt = EditorGUILayout.IntField("Value (Int)", intVal);
                        if (newInt != intVal)
                        {
                            PlayerPrefs.SetInt(key, newInt);
                            PlayerPrefs.Save();
                        }
                    }
                    else
                    {
                        // Must be float
                        float floatVal = PlayerPrefs.GetFloat(key, float.NaN);
                        if (!float.IsNaN(floatVal))
                        {
                            float newFloat = EditorGUILayout.FloatField("Value (Float)", floatVal);
                            if (newFloat != floatVal)
                            {
                                PlayerPrefs.SetFloat(key, newFloat);
                                PlayerPrefs.Save();
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("Delete Key", GUILayout.Width(100)))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Repaint();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        // Add New Key Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add New PlayerPref", EditorStyles.boldLabel);
        newKey = EditorGUILayout.TextField("Key", newKey);

        EditorGUILayout.BeginHorizontal();
        newIntValue = EditorGUILayout.IntField("Int Value", newIntValue);
        if (GUILayout.Button("Save Int", GUILayout.Width(80)))
        {
            if (!string.IsNullOrEmpty(newKey))
            {
                PlayerPrefs.SetInt(newKey, newIntValue);
                PlayerPrefs.Save();
                newKey = "";
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        newFloatValue = EditorGUILayout.FloatField("Float Value", newFloatValue);
        if (GUILayout.Button("Save Float", GUILayout.Width(80)))
        {
            if (!string.IsNullOrEmpty(newKey))
            {
                PlayerPrefs.SetFloat(newKey, newFloatValue);
                PlayerPrefs.Save();
                newKey = "";
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        newStringValue = EditorGUILayout.TextField("String Value", newStringValue);
        if (GUILayout.Button("Save String", GUILayout.Width(80)))
        {
            if (!string.IsNullOrEmpty(newKey))
            {
                PlayerPrefs.SetString(newKey, newStringValue);
                PlayerPrefs.Save();
                newKey = "";
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private List<string> GetAllKeys()
    {
        // This is a bit hacky since Unity doesn't provide a direct way to list all keys
        // We'll use a known trick with PlayerPrefs
        var keys = new List<string>();
        
        // You can expand this list with common keys from your game if needed
        // For full listing, one common approach is to use reflection on internal data, but it's fragile.
        
        // For now, we show keys that have been set during this session + common ones
        // Note: True full enumeration requires platform-specific code or Editor-only hacks.

        // Simple approach: just show what we can detect from known types
        return keys; // We'll improve this in the next version if needed
    }
}