using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueNode //: MonoBehaviour
{
    public int NodeID { get; set; }
    public Vector2 NodePosition { get; set; }
    public DialogueNode Parent { get; set; }
    public List<DialogueNode> Childen { get; set; } //= new List<DialogueNode>();
    public List<DialogueOption> DialogueOptions { get; set; }
    public string DialogueText { get; set; }
} 
