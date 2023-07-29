//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractItem : Interactive
{
    private int objectID;
    private AbstractCharacter character; //character who holds this item
    public delegate void RemoveFromMap(int ID);
    public static event RemoveFromMap OnRemove;

    public int ObjectID
    {
        get
        {
            return objectID;
        }
        set
        {
            objectID = value;
        }
    }

    public AbstractCharacter Character
    {
        get
        {
            return character;
        }
        set
        {
            character = value;
        }
    }

    private void Awake()
    {

    }

    protected void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.GetComponent<Player>() != null)
        {
            //if (Input.GetButtonDown("Button1"))
            {
                interact += AddToInventory;
                interact(collider.GetComponent<Player>());
            }
        }
    }

    public abstract void Use();

    public virtual void AddToInventory(AbstractCharacter character)
    {
        this.Character = character;
        if (OnRemove != null)
        {
            OnRemove(0);
        }
        //if character has room in inventory...
        character.Inventory[character.Inventory.Length - 1] = this;
        MapManager.Instance.RemoveItemFromMap(0);
        Destroy(this.gameObject);
    }
}