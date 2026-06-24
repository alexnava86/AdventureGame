// =============================================================================
// QuestEditorWindow.cs   —   Assets/Editor/
//
// A list-based editor for authoring the project's quests. Unlike the Dialogue
// Tree Editor (a branching graph), quests are mostly flat lists of objectives,
// so this is a scrollable, collapsible list — far simpler to use and maintain.
//
// It edits a QuestDatabase ScriptableObject directly (the same asset the
// Dialogue Tree Editor reads for its quest dropdowns), so there's no separate
// JSON file and no save/load code — Unity persists the asset for you.
//
// Open via:  Window ▸ Adventure Game ▸ Quest Editor
// =============================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DialogueEditor;

public class QuestEditorWindow : EditorWindow
{
    private QuestDatabase _db;
    private Vector2       _scroll;
    private readonly HashSet<int> _expandedQuests = new HashSet<int>();

    [MenuItem("Window/Tools/Quest Editor")]
    public static void Open()
    {
        var w = GetWindow<QuestEditorWindow>("Quest Editor");
        w.minSize = new Vector2(420, 400);
        w.TryAutoLoadDatabase();
    }

    private void OnEnable() => TryAutoLoadDatabase();

    /// <summary>Finds the first QuestDatabase in the project if none is assigned.</summary>
    private void TryAutoLoadDatabase()
    {
        if (_db != null) return;
        var guids = AssetDatabase.FindAssets("t:QuestDatabase");
        if (guids.Length > 0)
            _db = AssetDatabase.LoadAssetAtPath<QuestDatabase>(
                      AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    // =========================================================================
    // GUI
    // =========================================================================

    private void OnGUI()
    {
        DrawDatabaseField();
        if (_db == null)
        {
            EditorGUILayout.HelpBox(
                "No QuestDatabase assigned. Assign one above, or create it via " +
                "Assets ▸ Create ▸ Dialogue Editor ▸ Quest Database.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_db.quests == null) _db.quests = new List<Quest>();

        for (int i = 0; i < _db.quests.Count; i++)
            DrawQuest(i);

        EditorGUILayout.Space();
        if (GUILayout.Button("+ Add Quest", GUILayout.Height(26)))
        {
            Undo.RecordObject(_db, "Add Quest");
            _db.quests.Add(new Quest { questId = "new_quest", questName = "New Quest" });
            _expandedQuests.Add(_db.quests.Count - 1);
            MarkDirty();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawDatabaseField()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Quest Database", GUILayout.Width(95));
        var newDb = (QuestDatabase)EditorGUILayout.ObjectField(_db, typeof(QuestDatabase), false);
        if (newDb != _db) _db = newDb;
        EditorGUILayout.EndHorizontal();
    }

    // =========================================================================
    // Per-quest drawing
    // =========================================================================

    private void DrawQuest(int index)
    {
        var quest = _db.quests[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Header row: foldout + name + delete
        EditorGUILayout.BeginHorizontal();
        bool expanded = _expandedQuests.Contains(index);
        bool newExpanded = EditorGUILayout.Foldout(
            expanded, $"{quest.questName}   ({quest.questId})", true);
        if (newExpanded != expanded)
        {
            if (newExpanded) _expandedQuests.Add(index);
            else             _expandedQuests.Remove(index);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("✕", GUILayout.Width(22)))
        {
            if (EditorUtility.DisplayDialog("Delete Quest",
                $"Delete quest '{quest.questName}'?", "Delete", "Cancel"))
            {
                Undo.RecordObject(_db, "Delete Quest");
                _db.quests.RemoveAt(index);
                MarkDirty();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (_expandedQuests.Contains(index))
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            quest.questId      = EditorGUILayout.TextField("Quest ID", quest.questId);
            quest.questName    = EditorGUILayout.TextField("Name", quest.questName);
            quest.description  = EditorGUILayout.TextField("Description", quest.description);
            quest.isRepeatable = EditorGUILayout.Toggle("Repeatable", quest.isRepeatable);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Objectives", EditorStyles.boldLabel);

            if (quest.objectives == null) quest.objectives = new List<QuestObjective>();
            for (int o = 0; o < quest.objectives.Count; o++)
                DrawObjective(quest, o);

            if (GUILayout.Button("+ Add Objective"))
            {
                quest.objectives.Add(new QuestObjective { objectiveId = "objective_" + quest.objectives.Count });
                MarkDirty();
            }

            if (EditorGUI.EndChangeCheck()) MarkDirty();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawObjective(Quest quest, int oIndex)
    {
        var obj = quest.objectives[oIndex];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"#{oIndex}", GUILayout.Width(28));
        obj.objectiveId = EditorGUILayout.TextField(obj.objectiveId);
        if (GUILayout.Button("✕", GUILayout.Width(22)))
        {
            quest.objectives.RemoveAt(oIndex);
            MarkDirty();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        obj.description = EditorGUILayout.TextField("Description", obj.description);
        obj.type        = (ObjectiveType)EditorGUILayout.EnumPopup("Type", obj.type);

        // Only show target/count for the types that use them
        if (obj.type == ObjectiveType.CollectItem || obj.type == ObjectiveType.KillEnemy ||
            obj.type == ObjectiveType.ReachLocation)
        {
            string label = obj.type == ObjectiveType.CollectItem ? "Item Name"
                         : obj.type == ObjectiveType.KillEnemy    ? "Enemy Name"
                         : "Location ID";
            obj.targetId = EditorGUILayout.TextField(label, obj.targetId);
        }

        if (obj.type == ObjectiveType.CollectItem || obj.type == ObjectiveType.KillEnemy)
            obj.requiredCount = Mathf.Max(1, EditorGUILayout.IntField("Required Count", obj.requiredCount));

        obj.required = EditorGUILayout.Toggle("Required for completion", obj.required);

        // Prerequisite dropdown — pick another objective in this quest, or None
        DrawPrerequisiteDropdown(quest, obj, oIndex);

        EditorGUILayout.EndVertical();
    }

    private void DrawPrerequisiteDropdown(Quest quest, QuestObjective obj, int selfIndex)
    {
        var ids = new List<string> { "(none)" };
        for (int i = 0; i < quest.objectives.Count; i++)
            if (i != selfIndex) ids.Add(quest.objectives[i].objectiveId);

        int current = 0;
        for (int i = 1; i < ids.Count; i++)
            if (ids[i] == obj.prerequisiteObjectiveId) { current = i; break; }

        int picked = EditorGUILayout.Popup("Prerequisite", current, ids.ToArray());
        obj.prerequisiteObjectiveId = picked == 0 ? "" : ids[picked];
    }

    // =========================================================================
    // Persistence
    // =========================================================================

    private void MarkDirty()
    {
        if (_db == null) return;
        EditorUtility.SetDirty(_db);
        // Quests are pure data; saving the asset writes them to disk.
        AssetDatabase.SaveAssetIfDirty(_db);
    }
}
