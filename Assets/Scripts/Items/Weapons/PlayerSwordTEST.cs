using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordTEST : MonoBehaviour
{
    public int swordOffense;
    
    public delegate void PlayerAction<T, T2>(T action, T2 parameter2);
    public static event PlayerAction<Int32, AbstractCharacter> OnWeaponContact;
    //public static event PlayerAction<Int32> OnWeaponContact;
    
    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Enemy>() != null)
        {
            if (OnWeaponContact != null)
            {
                //OnWeaponContact(swordOffense);
                OnWeaponContact(swordOffense, collider.GetComponent<Enemy>());
            }
            if (collider.GetComponent<ColorBlinker>() != null)
            {
                collider.GetComponent<ColorBlinker>().enabled = true;
            }
        }
    }
}
