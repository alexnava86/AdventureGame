using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField]
    private Sprite[] hpSprites;

    void Start()
    {
        //Note: Revise this later to set the SortingLayer / SortingLayerOrder on the same layer as UI?
        if (this.GetComponent<SpriteRenderer>() != null && MapManager.Instance.map != null)
        {
            this.GetComponent<SpriteRenderer>().sortingOrder = MapManager.Instance.groundLayerID;
        }
    }

    void OnEnable()
    {
        Enemy.OnEnemyDamage += UpdateHpBar;
        //Enemy.OnDeath += DestroyHPBar;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDamage -= UpdateHpBar;
    }

    void UpdateHpBar(int hpPercentage, AbstractCharacter character)
    {
        if (character == this.GetComponentInParent<AbstractCharacter>())
        {
            //Debug.Log(hpPercentage);
            int animID = hpSprites.Length - (hpPercentage / 4);
            Debug.Log(animID);
            this.GetComponent<SpriteRenderer>().sprite = hpSprites[animID];
        }
    }
    void DestroyHPBar(AbstractCharacter character)
    {
        Destroy(this.gameObject);
    }
}