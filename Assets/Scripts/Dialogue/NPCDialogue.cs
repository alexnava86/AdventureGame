// =============================================================================
// NPCDialogue.cs
// Place in: Assets/Scripts/NPCs/
//
// Attach to any NPC GameObject alongside NPC.cs.
// Drag a .json dialogue tree (saved from the Dialogue Tree Editor) into
// the "Dialogue Tree Json" field in the Inspector.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;

/// <summary>
/// Loads a dialogue tree from JSON and exposes a runtime API for your
/// dialogue controller to drive conversations.
///
/// Name / icon cascade (highest → lowest priority):
///   DialogueNode.characterName → DialogueTree.characterName → NPC.characterName → GameObject.name
///   DialogueNode.iconGuid      → DialogueTree.characterIconGuid → NPC.characterIcon
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("Drag a .json dialogue tree saved by the Dialogue Tree Editor here.")]
    public TextAsset dialogueTreeJson;

    [Header("Character")]
    [Tooltip("NPC component on this GameObject. Auto-located on Awake if not assigned.")]
    public NPC npcCharacter;

    private DialogueTreeData _tree;

    private void Awake()
    {
        if (npcCharacter == null)
            npcCharacter = GetComponent<NPC>();
    }

    // ── Tree ─────────────────────────────────────────────────────────────────

    public DialogueTreeData Tree
    {
        get
        {
            if (_tree == null && dialogueTreeJson != null)
                _tree = JsonUtility.FromJson<DialogueTreeData>(dialogueTreeJson.text);
            return _tree;
        }
    }

    public bool HasDialogue => GetRootNode() != null;

    // ── Display helpers ───────────────────────────────────────────────────────

    public string GetDisplayName(DialogueNodeData node = null)
    {
        if (node != null && !string.IsNullOrEmpty(node.characterName))   return node.characterName;
        if (Tree != null && !string.IsNullOrEmpty(Tree.characterName))   return Tree.characterName;
        if (npcCharacter != null && !string.IsNullOrEmpty(npcCharacter.characterName)) return npcCharacter.characterName;
        return gameObject.name;
    }

    /// <summary>
    /// Per-node and tree-level iconGuids are AssetDatabase GUIDs (editor previews).
    /// For runtime loading store the Resources-relative path instead (no extension).
    /// Falls back to NPC.characterIcon — the most reliable runtime source.
    /// </summary>
    public Sprite GetDisplayIcon(DialogueNodeData node = null)
    {
        if (node != null && !string.IsNullOrEmpty(node.iconGuid))
        { var s = Resources.Load<Sprite>(node.iconGuid); if (s != null) return s; }

        if (Tree != null && !string.IsNullOrEmpty(Tree.characterIconGuid))
        { var s = Resources.Load<Sprite>(Tree.characterIconGuid); if (s != null) return s; }

        return npcCharacter?.characterIcon;
    }

    // ── Runtime navigation API ────────────────────────────────────────────────

    public DialogueNodeData   GetRootNode()     => Tree?.GetRoot();
    public DialogueNodeData   GetNode(int id)   => Tree?.GetNode(id);
    public DialogueOptionData GetOption(int id) => Tree?.GetOption(id);

    public List<DialogueOptionData> GetOptions(DialogueNodeData node)
    {
        var result = new List<DialogueOptionData>();
        if (node?.optionIds == null || Tree == null) return result;
        foreach (int id in node.optionIds)
        { var opt = Tree.GetOption(id); if (opt != null) result.Add(opt); }
        return result;
    }

    public DialogueNodeData GetNextNode(DialogueNodeData current)
    {
        if (current == null || current.nextNodeId < 0 || Tree == null) return null;
        return Tree.GetNode(current.nextNodeId);
    }

    public DialogueNodeData GetNextNodeFromOption(DialogueOptionData option)
    {
        if (option == null || option.nextNodeId < 0 || Tree == null) return null;
        return Tree.GetNode(option.nextNodeId);
    }

    public void ReloadTree() { _tree = null; _ = Tree; }
}
