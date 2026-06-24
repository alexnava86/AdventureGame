using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonetaryDropItem : DropItem
{
    public int monetaryValue;
    public override void Use(AbstractCharacter character)
    {
        base.Use(character);
        if (character != null)
        {
            if (character.CoinPurse + monetaryValue < 9999)
            {
                character.CoinPurse += monetaryValue;
            }
            else if(character.CoinPurse + monetaryValue > 9999)
            {
                character.CoinPurse = 9999;
            }
            character.CoinUpdate(monetaryValue);
        }
    }
}
