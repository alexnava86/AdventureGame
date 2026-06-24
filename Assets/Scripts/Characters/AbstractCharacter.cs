//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class AbstractCharacter : MonoBehaviour, ITakeDamage
{
    #region Variables
    public List<AbstractItem> Inventory = new List<AbstractItem>(new AbstractItem[20]);
    protected CSVReader data;
    #endregion

    #region Properties
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public int Endurance { get; set; }
    public int MaxEndurance { get; set; }
    public int PhysicalAttack { get; set; }
    public int PhysicalDefense { get; set; }
    public int MagicAttack { get; set; }
    public int MagicDefense { get; set; }
    public int ExpToLvlUp { get; set; }
    public int CoinPurse { get; set; }
    //public AbstractItem[] Inventory { get; set; }
    //public Skill[] Skills { get; set; }
    //public Spell[] Spells { get; set; }
    public enum StatusEffect { Bleed, Poisoned, Sick, Burn, Heatstroke, Frostbite, Armstrong, Ironflesh };
    public delegate void CharacterAction();
    public delegate void CharacterAction<T>(T action);
    public static event CharacterAction<Int32> OnHpUpdate;
    public static event CharacterAction<Int32> OnCoinUpdate;
    public static event CharacterAction<AbstractCharacter> OnDeath;

    #endregion

    #region MonoBehaviour
    protected virtual void Start()
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<TerrainVolume>() != null)
        {
            /*
            if (OnWaterContact != null)
            {
                OnWaterContact(1);
            }
            if (collider.GetComponent<ColorBlinker>() != null)
            {
                collider.GetComponent<ColorBlinker>().enabled = true;
            }
            */
            Debug.Log("WATER!");
        }
    }
    #endregion

    #region Methods
    protected void SetLevelData(int Level)
    {
        data = this.GetComponent<CSVReader>();
        this.MaxHp = Convert.ToInt32(data.grid[Level, 1]);
        this.MaxMp = Convert.ToInt32(data.grid[Level, 2]);
        this.MaxEndurance = Convert.ToInt32(data.grid[Level, 3]);
        this.PhysicalAttack = Convert.ToInt32(data.grid[Level, 4]);
        this.PhysicalDefense = Convert.ToInt32(data.grid[Level, 5]);
        this.MagicAttack = Convert.ToInt32(data.grid[Level, 6]);
        this.MagicDefense = Convert.ToInt32(data.grid[Level, 7]);
        this.ExpToLvlUp = Convert.ToInt32(data.grid[Level, 8]);
    }

    public void SetAnim(string animName)
    {
        if (this.GetComponent<Animator>() != null)
        {
            Animator anim = this.GetComponent<Animator>();
            IEnumerable<string> state = from s in anim.parameters where s.name != animName select s.name;

            foreach (string s in state)
            {
                anim.SetBool(s, false);
            }
            anim.SetBool(animName, true);
        }
    }

    public void TakeDamage(int value)
    {
        this.Hp -= value;
        if (this.Hp <= 0) //Player Death
        {
            OnDeath(this);
            Destroy(this.gameObject);
        }
    }
    
    public int HpUpdate(int value)
    {
        float hpRatio;
        int hpPercent;

        hpRatio = ((float)this.Hp / (float)this.MaxHp) * 100f;
        hpPercent = (int)hpRatio;
        if (OnHpUpdate != null)
        {
            OnHpUpdate(hpPercent);
        }
        if (this.Hp <= 0) //Player/Character Death
        {
            //OnDeath(this);
            Destroy(this.gameObject);
        }
        return this.Hp;
    }

    public int CoinUpdate(int value)
    {

        if (OnCoinUpdate != null)
        {
            OnCoinUpdate(value);
        }
        return this.CoinPurse;
        
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
            statusLibrary.Add(StatusEffect.Bleed, new Bleed());
            statusLibrary.Add(StatusEffect.Sick, new Sick());
            statusLibrary.Add(StatusEffect.Burn, new Burn());
            statusLibrary.Add(StatusEffect.Heatstroke, new Heatstroke());
            statusLibrary.Add(StatusEffect.Frostbite, new Frostbite());
            statusLibrary.Add(StatusEffect.Armstrong, new Armstrong());
            statusLibrary.Add(StatusEffect.Ironflesh, new Ironflesh());
        }
        public static void Effect(StatusEffect effectName)
        {
            statusLibrary[effectName].Effect(effectName);
        }
        public static void Cure()
        {

        }
    }
    private class Bleed : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Poisoned : IStatus
    {
        public void Effect(StatusEffect effectName)
        {
            Debug.Log("Im friggin' poisoned! "); // wrks
                                                 //Determine the damage of poison effect here, display damage text
                                                 //Display any visual effects of poison to character / instantiate partile effects
        }
        public void Cure()
        {

        }
    }
    private class Sick : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Burn : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Heatstroke : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Frostbite : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Armstrong : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    private class Ironflesh : IStatus
    {
        public void Effect(StatusEffect effectName)
        {

        }
        public void Cure()
        {

        }
    }
    #endregion
}