using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class EnemyTEST : AbstractCharacter
{
    public delegate void EnemyAction(int x);
    public static event EnemyAction OnCharacterContact;
    public delegate void EnemyAction<T>(T action);
    public static event EnemyAction<Int32> OnEnemyDamage;

    private void Start()
    {
        base.Start();
        this.SetLevelData(1);
        this.Hp = this.MaxHp;
        this.Mp = this.MaxMp;
        this.Endurance = this.MaxEndurance;
    }
    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponent<AbstractCharacter>() != null)
        {
            if(OnCharacterContact != null)
            { 
                OnCharacterContact(1);
            }
            if (collider.GetComponent<ColorBlinker>() != null)
            {
                collider.GetComponent<ColorBlinker>().enabled = true;
            }
        }
    }

    protected void OnEnable()
    {
        PlayerSword.OnWeaponContact += Damage;
    }

    protected void OnDisable()
    {
        PlayerSword.OnWeaponContact -= Damage;
    }

    private void Damage(int damage, AbstractCharacter sender)
    {
        if(sender == this.AbstractCharacter)
        {
            float hpRatio;// = ((float)this.Hp / (float)this.MaxHp) * 100f;
            int hpPercent;// = (int)hpRatio;
            base.Damage(damage);
            hpRatio = ((float)this.Hp / (float)this.MaxHp) * 100f;
            hpPercent = (int)hpRatio;
            if (OnEnemyDamage != null)
            {
                OnEnemyDamage(hpPercent);
            }
        }
    }
}