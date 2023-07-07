using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDCanvas : MonoBehaviour
{
    public GameObject HpBar;
    public GameObject MpBar;
    public GameObject EnduranceBar;
    public Sprite[] hpSprites;
     
    void Start()
    {
        
    }

    void OnEnable()
    {
        Player.OnPlayerDamage += UpdateHpBar;
    }

    void OnDisable()
    {
        Player.OnPlayerDamage -= UpdateHpBar;
    }

    void UpdateHpBar(int hpPercentage)
    {

        int animID = (hpSprites.Length - 1) - hpPercentage / 2;
        //Debug.Log(hpPercentage);
        this.HpBar.GetComponent<Image>().sprite = hpSprites[animID];
    }
}
