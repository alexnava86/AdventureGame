﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractCharacter : MonoBehaviour
{
    #region Variables
    public List<Color32> mainColor = new List<Color32>();
    public List<Color32> subColor = new List<Color32>();
    public List<Color32> accentColor = new List<Color32>();
    #endregion

    #region Properties
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public int PhysicalAttack { get; set; }
    public int PhysicalDefense { get; set; }
    public int MagicAttack { get; set; }
    public int MagicDefense { get; set; }
    //public Item[] Inventory { get; set; }
    //public Skill[] Skills { get; set; }
    //public Spell[] Spells { get; set; }
    //public enum StatusEffect { Poisoned, Silenced, Sleeping, Charmed, Hypnotized, Blinded, Confused, Paralyzed };
    //public enum StatusEffect { Poisoned, Silenced, Sleeping, Charmed, Hypnotized, Blinded, Confused, Paralyzed };
    #endregion


    #region MonoBehaviour
    public void Start()
    {
        if (this.GetComponent<SpriteRenderer>() != null && MapManager.Instance.map != null)
        {
            this.GetComponent<SpriteRenderer>().sortingOrder = MapManager.Instance.groundLayerID;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region Methods
    protected void SetLevelData(int Level)
    {
        /*
        data = this.GetComponent<CSVReader>();
        this.MaxHp = Convert.ToInt32(data.grid[Level, 1]);
        this.MaxMp = Convert.ToInt32(data.grid[Level, 2]);
        this.PhysicalAttack = Convert.ToInt32(data.grid[Level, 3]);
        this.PhysicalDefense = Convert.ToInt32(data.grid[Level, 4]);
        this.MagicAttack = Convert.ToInt32(data.grid[Level, 5]);
        this.MagicDefense = Convert.ToInt32(data.grid[Level, 6]);
        */
    }

    public virtual void ExecuteActiveStatusEffects()
    {
        /*
        foreach (StatusEffect effect in this.StatusEffects)
        {
            Status.Effect(effect);
        }
        */
    }
    #endregion

    #region Interfaces
    /*
    public interface IStatus
    {
        void Effect(StatusEffect effectName);//AbstractCharacter character)
        void Cure();
    }
    protected class Status
    {
        private static Dictionary<StatusEffect, IStatus> statusLibrary = new Dictionary<StatusEffect, IStatus>();
        //private static List<IStatus> statusIndex = new List<IStatus>();
        static Status()
        {
            statusLibrary.Add(StatusEffect.Poisoned, new Poisoned());
            statusLibrary.Add(StatusEffect.Silenced, new Silenced());
            statusLibrary.Add(StatusEffect.Sleeping, new Sleeping());
            statusLibrary.Add(StatusEffect.Charmed, new Charmed());
            statusLibrary.Add(StatusEffect.Hypnotized, new Hypnotized());
            statusLibrary.Add(StatusEffect.Blinded, new Blinded());
            statusLibrary.Add(StatusEffect.Confused, new Confused());
            statusLibrary.Add(StatusEffect.Paralyzed, new Paralyzed());
        }
        public static void Effect(StatusEffect effectName)
        {
            statusLibrary[effectName].Effect(effectName);
        }
        public static void Cure()
        {

        }
    }
    private class Poisoned : IStatus
    {
        public void Effect(StatusEffect effectName)
        {
            Debug.Log("Im friggin' poisoned! "); // wrks
                                                 //Determine the damage of poison effect here, display damage text
                                                 //Display any visual effects of poison to character
        }
        public void Cure()
        {

        }
    }
    private class Silenced : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Sleeping : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Charmed : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Hypnotized : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Blinded : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Confused : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Paralyzed : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    */
    #endregion

}
