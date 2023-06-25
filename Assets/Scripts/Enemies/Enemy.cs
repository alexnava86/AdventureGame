using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AbstractCharacter
{
    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            collider.GetComponent<Player>().Damage(1);
            //Debug.Log(GameManager.Instance.gameObject.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite);// = "";
        }
    }
}