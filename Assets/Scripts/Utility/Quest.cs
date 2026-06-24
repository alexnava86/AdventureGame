// =============================================================================
// Quest.cs   —   Assets/Resources/Scripts/Utilities/
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueEditor
{
    [Serializable]
    public class Quest
    {
        [Tooltip("Unique identifier used everywhere quest events reference this quest. Never change this after production use.")]
        public string questId     = "";
        public string questName   = "";
        public string description = "";
        [Tooltip("When true a completed quest can be started again.")]
        public bool isRepeatable  = false;

        [Tooltip("The steps that make up this quest. The quest completes when every " +
                 "REQUIRED objective is complete.")]
        public List<QuestObjective> objectives = new List<QuestObjective>();

        // ── Helpers ──────────────────────────────────────────────────────────

        public QuestObjective GetObjective(string objectiveId) =>
            objectives?.Find(o => o.objectiveId == objectiveId);

        /// <summary>True when every required objective is complete.</summary>
        public bool AllRequiredComplete()
        {
            if (objectives == null || objectives.Count == 0) return true;
            foreach (var o in objectives)
                if (o.required && !o.completed) return false;
            return true;
        }

        public void ResetAllObjectives()
        {
            if (objectives == null) return;
            foreach (var o in objectives) o.ResetProgress();
        }
    }
}
