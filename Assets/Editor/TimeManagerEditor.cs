// =============================================================================
// TimeManagerEditor.cs   —   Assets/Editor/
//
// Adds quick time-of-day preset buttons to the TimeManager Inspector so you can
// jump to dawn/noon/dusk/midnight with one click while tuning the look, plus a
// read-out of the current clock time. Works in edit mode (no Play needed).
// =============================================================================

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimeManager))]
public class TimeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var tm = (TimeManager)target;

        // Default fields (filterImage, currentHour slider, gradient, curve)
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Current clock readout
        int h = Mathf.FloorToInt(tm.currentHour) % 24;
        int m = Mathf.FloorToInt((tm.currentHour - Mathf.Floor(tm.currentHour)) * 60f);
        string ampm = h < 12 ? "AM" : "PM";
        int h12 = h % 12; if (h12 == 0) h12 = 12;
        EditorGUILayout.LabelField("Clock", $"{h12}:{m:00} {ampm}", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);

        // Each button is exactly 1/3 of the inspector width and the same fixed height,
        // so all six are uniform across both rows.
        float third = (EditorGUIUtility.currentViewWidth - 26f) / 3f;
        var btn = new GUILayoutOption[] { GUILayout.Width(third), GUILayout.Height(38) };

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Midnight\n12 AM", btn)) SetHour(tm, 0f);
        if (GUILayout.Button("Dawn\n6 AM",      btn)) SetHour(tm, 6f);
        if (GUILayout.Button("Morning\n9 AM",   btn)) SetHour(tm, 9f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Noon\n12 PM",     btn)) SetHour(tm, 12f);
        if (GUILayout.Button("Dusk\n6 PM",      btn)) SetHour(tm, 18f);
        if (GUILayout.Button("Night\n10 PM",    btn)) SetHour(tm, 22f);
        EditorGUILayout.EndHorizontal();
    }

    private void SetHour(TimeManager tm, float hour)
    {
        Undo.RecordObject(tm, "Set Time of Day");
        tm.currentHour = hour;
        tm.ApplyFilter();
        EditorUtility.SetDirty(tm);
        if (tm.filterImage != null) EditorUtility.SetDirty(tm.filterImage);
    }
}
