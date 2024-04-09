using System.Collections;
using System.Collections.Generic;
//using SimpleJSON;
using UnityEngine;

public class DialogueTree //: MonoBehaviour
{
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
    //private JSONNode json = new JSONNode();

    public void AddDialogueNode()
    {
        //Add a Dialogue Node to the current temp dialogue tree
        DialogueNode node = new DialogueNode();
        dialogueNodes.Add(node);
    }
    public void RemoveDialogueNode(int nodeID)
    {
        //dialogueNodes.Remove(node);
    }
}
