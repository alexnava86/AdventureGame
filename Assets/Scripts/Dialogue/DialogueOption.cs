// =============================================================================
// DialogueOption.cs
// Place in: Assets/DialogueEditor/Scripts/
// =============================================================================

using System;
using System.Collections.Generic;

namespace DialogueEditor
{
    /// <summary>
    /// Represents a player response choice inside a dialogue exchange.
    /// A DialogueOption's parent must always be a DialogueNode (never another option).
    /// It optionally leads to a follow-up DialogueNode; if nextNodeId is -1 the
    /// conversation ends when this option is chosen.
    /// </summary>
    [Serializable]
    public class DialogueOptionData
    {
        public int    id;
        public string optionText    = "Option";
        public float  xPos          = 200f;
        public float  yPos          = 300f;

        /// <summary>The DialogueNode that presents this option. -1 = orphan (unattached).</summary>
        public int parentNodeId = -1;

        /// <summary>The DialogueNode this option leads to. -1 = end of conversation.</summary>
        public int nextNodeId   = -1;

        /// <summary>
        /// Quest events fired when the player chooses this option.
        /// Processed by QuestManager.TriggerEvents() in your dialogue controller.
        /// </summary>
        public List<QuestEventData> questEvents = new List<QuestEventData>();
    }
}
