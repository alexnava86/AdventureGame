using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DropItem : AbstractItem
{
    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            interact += Use;
            interact(collider.GetComponent<Player>());
        }
    }
}