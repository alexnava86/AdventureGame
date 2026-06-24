// =============================================================================
// DialogueNode.cs
// Place in: Assets/DialogueEditor/Scripts/
// =============================================================================

using System;
using System.Collections.Generic;

namespace DialogueEditor
{
    /// <summary>
    /// Represents a single NPC speech beat in the dialogue tree.
    /// A DialogueNode either connects directly to one other DialogueNode (nextNodeId)
    /// OR presents a list of player DialogueOptions — never both at once.
    /// </summary>
    [Serializable]
    public class DialogueNodeData
    {
        public int    id;
        public bool   isRoot;
        public string characterName  = "";
        public string dialogueText   = "";
        public float  xPos           = 200f;
        public float  yPos           = 150f;

        /// <summary>
        /// ID of the next DialogueNode for direct (no-options) flow. -1 = none.
        /// Mutually exclusive with optionIds.
        /// </summary>
        public int nextNodeId = -1;

        /// <summary>
        /// IDs of the DialogueOptions this node presents to the player.
        /// Mutually exclusive with nextNodeId.
        /// </summary>
        public List<int> optionIds = new List<int>();

        /// <summary>
        /// Optional per-node icon override (Unity AssetDatabase GUID of a Sprite).
        /// When set, this node displays this icon instead of the tree-level character icon.
        /// Useful for showing attitude/emotion changes as dialogue progresses.
        /// Leave empty to use the tree's default characterIconGuid.
        /// NOTE: Runtime use requires the sprite to be in a Resources folder.
        /// </summary>
        public string iconGuid = "";

        /// <summary>
        /// Quest events fired when this node is reached during dialogue.
        /// Processed by QuestManager.TriggerEvents() in your dialogue controller.
        /// </summary>
        public List<QuestEventData> questEvents = new List<QuestEventData>();
    }
}
