using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShroomsBlue : DropItem
{
    public override void Use(AbstractCharacter character)
    {
        base.Use(character);
        if (Character != null)
        {
            //Character.Hp += 10;
        }
    }
}
