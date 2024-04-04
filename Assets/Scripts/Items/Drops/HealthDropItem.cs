using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDropItem : DropItem
{
    public int healthValue;
    public override void Use(AbstractCharacter character)
    {
        base.Use(character);
        if (character != null)
        {
            if (character.Hp + healthValue < character.MaxHp)
            {
                character.Hp += healthValue;
            }
            else
            {
                character.Hp = character.MaxHp;
            }
            character.HpUpdate(healthValue);
        }
    }
}
