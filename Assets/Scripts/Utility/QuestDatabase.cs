// =============================================================================
// QuestDatabase.cs
// Place in: Assets/DialogueEditor/Scripts/
//
// Create via: Assets ▸ Create ▸ Dialogue Editor ▸ Quest Database
// Assign it to the QuestManager component in your scene.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;

/// <summary>
/// ScriptableObject that stores every Quest definition for the project.
/// The Dialogue Tree Editor reads this to show quest name dropdowns when
/// authoring quest events on nodes — no database means plain text-field entry.
/// </summary>
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Dialogue Editor/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<Quest> quests = new List<Quest>();

    // ------------------------------------------------------------------
    // Lookup helpers
    // ------------------------------------------------------------------

    /// <summary>Returns the Quest with the given id, or null if not found.</summary>
    public Quest GetQuest(string questId) =>
        quests?.Find(q => q.questId == questId);

    /// <summary>Returns true when a Quest with the given id exists.</summary>
    public bool QuestExists(string questId) =>
        quests != null && quests.Exists(q => q.questId == questId);
}
