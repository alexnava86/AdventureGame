using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOption //: MonoBehaviour
{
    public string OptionText;
    public Vector2 NodePosition { get; set; }
    public delegate void OptionAction();
    public static event OptionAction OnOptionSelect;
}
