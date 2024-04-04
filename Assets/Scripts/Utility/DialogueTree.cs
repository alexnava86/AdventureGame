using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTree //: MonoBehaviour
{
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

    public void AddDialogueNode(DialogueNode node)
    {
        dialogueNodes.Add(node);
    }
    public void RemoveDialogueNode(DialogueNode node)
    {
        dialogueNodes.Remove(node);
    }
}
