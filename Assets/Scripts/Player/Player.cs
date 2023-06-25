using UnityEngine;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;

public class Player : AbstractCharacter
{
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
    #endregion

    #region Methods
    private void Damage(int damage)
    {
        base.Damage(damage);
        //GameManager.Instance.gameObject.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = ;
    }
    #endregion

    #region Coroutines

    #endregion
}