using System;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : AbstractCharacter
{
    public delegate void PlayerAction<T>(T action);
    public static event PlayerAction<Int32> OnPlayerDamage;
    public static event PlayerAction<Int32> OnPlayerHeal;

    #region Variables
    public List<Sprite> hpBar = new List<Sprite>();
    #endregion

    #region Properties
    #endregion

    #region MonoBehaviour
    private new void Start()
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
        Enemy.OnCharacterContact += Damage;
        Portal.OnPlayerSummon += MovePlayerToPortal;
        //DropItem.OnCharacterTouch += Heal;
    }
    void OnDisable()
    {
        Enemy.OnCharacterContact -= Damage;
        Portal.OnPlayerSummon -= MovePlayerToPortal;
        //DropItem.OnCharacterContact -= Heal;
    }
    #endregion

    #region Methods
    private new void Damage(int value)
    {
        float hpRatio; // = ((float)this.Hp / (float)this.MaxHp) * 100f;
        int hpPercent; // = (int)hpRatio;

        base.Damage(value);
        hpRatio = ((float)this.Hp / (float)this.MaxHp) * 100f;
        hpPercent = (int)hpRatio;
        if (OnPlayerDamage != null)
        {
            OnPlayerDamage(hpPercent);
        }
    }
    private void MovePlayerToPortal(Vector3 pos)
    {
        //Debug.Log(pos);
        //this.transform.position = pos;
    }
    #endregion

    #region Coroutines

    #endregion
}