using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int swordOffense;
    
    public delegate void PlayerAction<T>(T action);
    public static event PlayerAction<Int32> OnContact;

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Enemy>() != null)
        {
            if (OnContact != null)
            {
                OnContact(swordOffense);
            }
            if (collider.GetComponent<ColorBlinker>() != null)
            {
                collider.GetComponent<ColorBlinker>().enabled = true;
            }
        }
    }
}
