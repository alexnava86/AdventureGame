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
    public void AddDialogueNode(string text)
    {
        //Add a Dialogue Node to the current temp dialogue tree
        DialogueNode node = new DialogueNode();
        dialogueNodes.Add(node);
        node.DialogueText = text;
    }
    public void RemoveDialogueNode(int nodeID)
    {
        // Find the index of the node with the given nodeID
        int indexToRemove = dialogueNodes.FindIndex(node => node.NodeID == nodeID);

        if (indexToRemove != -1)
        {
            // Remove the node at the found index
            dialogueNodes.RemoveAt(indexToRemove);
        }
        else
        {
            Debug.LogWarning("Node with NodeID " + nodeID + " not found.");
        }
    }
}
