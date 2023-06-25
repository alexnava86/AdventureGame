using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleRed : AbstractItem
{
    public override void Use()
    {
        if (Character != null)
        {
            Character.Hp += 10;
        }
    }
}
