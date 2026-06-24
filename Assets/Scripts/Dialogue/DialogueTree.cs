// =============================================================================
// DialogueTree.cs
// Place in: Assets/DialogueEditor/Scripts/
// =============================================================================

using System;
using System.Collections.Generic;

namespace DialogueEditor
{
    /// <summary>
    /// Root container for an entire dialogue tree. Serializes to/from JSON via JsonUtility.
    /// </summary>
    [Serializable]
    public class DialogueTreeData
    {
        public string treeName             = "New Dialogue Tree";
        public int    nextId               = 1;
        public int    maxOptionsPerNode    = 4;      // max DialogueOptions per DialogueNode
        public bool   rootAtTop            = true;   // true = tree flows top-to-bottom

        /// <summary>
        /// Optional display-name override for this entire conversation.
        /// Overrides NPCCharacterData.characterName while this tree is active.
        /// Leave empty to use the NPC's canonical name from NPCCharacterData.
        /// </summary>
        public string characterName        = "";

        /// <summary>
        /// Unity AssetDatabase GUID of the portrait Sprite for this conversation.
        /// Overrides NPCCharacterData.characterIcon while this tree is active.
        /// Leave empty to use the NPC's canonical icon from NPCCharacterData.
        /// </summary>
        public string characterIconGuid    = "";

        public List<DialogueNodeData>   dialogueNodes   = new List<DialogueNodeData>();
        public List<DialogueOptionData> dialogueOptions = new List<DialogueOptionData>();

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        public int AllocateId() => nextId++;

        public DialogueNodeData   GetNode(int id)   => dialogueNodes?.Find(n => n.id == id);
        public DialogueOptionData GetOption(int id) => dialogueOptions?.Find(o => o.id == id);

        /// <summary>Returns the root node (isRoot == true), or the first node if none marked.</summary>
        public DialogueNodeData GetRoot()
        {
            if (dialogueNodes == null || dialogueNodes.Count == 0) return null;
            return dialogueNodes.Find(n => n.isRoot) ?? dialogueNodes[0];
        }
    }
}
