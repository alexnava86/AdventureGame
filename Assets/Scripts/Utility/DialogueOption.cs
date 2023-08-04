using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

public class DialogueOption //: MonoBehaviour
{
    public string OptionText;
    public delegate void OptionAction();
    public static event OptionAction OnOptionSelect;
}
