// =============================================================================
// QuestObjective.cs   —   Assets/Resources/Scripts/Utilities/  (with Quest.cs)
//
// A single step within a Quest. A Quest is complete when all of its REQUIRED
// objectives are complete. Objectives can be completed by different triggers —
// talking to an NPC (via dialogue Quest Events), picking up an item, killing a
// number of a certain enemy, or a manual/custom call from any system.
// =============================================================================

using System;
using UnityEngine;

namespace DialogueEditor
{
    /// <summary>How an objective gets marked complete.</summary>
    public enum ObjectiveType
    {
        Talk,         // completed by a dialogue Quest Event (or manual call)
        CollectItem,  // completed by picking up `targetId` item(s), `requiredCount` times
        KillEnemy,    // completed by killing `requiredCount` of enemy named `targetId`
        ReachLocation,// completed when the player enters a trigger that reports `targetId`
        Custom        // completed only by an explicit CompleteObjective() call
    }

    [Serializable]
    public class QuestObjective
    {
        [Tooltip("Unique ID within its parent quest (e.g. 'collect_herbs').")]
        public string objectiveId = "";

        [Tooltip("Shown to the player in a quest log (e.g. 'Collect 3 moonherbs').")]
        public string description = "";

        [Tooltip("How this objective is completed.")]
        public ObjectiveType type = ObjectiveType.Custom;

        [Tooltip("What this objective targets: an item name, an enemy name, or a " +
                 "location id — depending on Type. Unused for Talk/Custom.")]
        public string targetId = "";

        [Tooltip("How many times the trigger must happen (items collected, enemies " +
                 "killed). Use 1 for talk/reach/custom objectives.")]
        public int requiredCount = 1;

        [Tooltip("Optional: this objective stays locked until the objective with this " +
                 "ID (in the same quest) is complete. Leave empty for no prerequisite. " +
                 "Covers simple 'do A before B' chains without a full graph.")]
        public string prerequisiteObjectiveId = "";

        [Tooltip("If false, this objective is optional and does NOT block quest completion.")]
        public bool required = true;

        // ── Runtime progress (not authored; tracked while playing) ────────────
        [NonSerialized] public int  currentCount;
        [NonSerialized] public bool completed;

        public void ResetProgress() { currentCount = 0; completed = false; }
    }
}
