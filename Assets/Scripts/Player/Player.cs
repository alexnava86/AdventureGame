using System;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : AbstractCharacter
{
    public delegate void PlayerAction<T>(T action);
    public static event PlayerAction<Int32> OnPlayerDamage;

    #region Variables
    public List<Sprite> hpBar = new List<Sprite>();
    #endregion

    #region Properties
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        base.Start();
        this.SetLevelData(1);
        this.Hp = this.MaxHp;
        this.Mp = this.MaxMp;
        this.Endurance = this.MaxEndurance;
    }

    private void Update()
    {

    }
    public void FixedUpdate()
    {

    }
    void OnEnable()
    {
        Enemy.OnCharacterTouch += Damage;
    }


    void OnDisable()
    {
        Enemy.OnCharacterTouch -= Damage;
    }
    #endregion

    #region Methods
    private void Damage(int damage)
    {
        float hpRatio;// = ((float)this.Hp / (float)this.MaxHp) * 100f;
        int hpPercent;// = (int)hpRatio;

        base.Damage(damage);
        hpRatio = ((float)this.Hp / (float)this.MaxHp) * 100f;
        hpPercent = (int)hpRatio;
        if (OnPlayerDamage != null)
        {
            OnPlayerDamage(hpPercent);
        }
    }
    #endregion

    #region Coroutines

    #endregion
}