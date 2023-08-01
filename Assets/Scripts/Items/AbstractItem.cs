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

    public virtual void Use(AbstractCharacter character)
    {
        this.Character = character;
        Destroy(this.gameObject);
    }

    public virtual void AddToInventory(AbstractCharacter character)
    {
        this.Character = character;
        if (OnRemove != null)
        {
            OnRemove(0);
        }
        //If character has room in their 'Inventory' List/Array
        if (character.Inventory.Contains(null))
        {
            character.Inventory.Insert(character.Inventory.IndexOf(null), this);
            //Debug.Log("InventorySlot=" + character.Inventory.IndexOf(null) + " Item=" + this.GetType().ToString());
        }
        //MapManager.Instance.RemoveItemFromMap(0); //this.ObjectID; ?
        Destroy(this.gameObject);
    }
}