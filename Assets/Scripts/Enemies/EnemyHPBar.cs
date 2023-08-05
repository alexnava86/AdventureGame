using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField]
    private Sprite[] hpSprites;
    [SerializeField]

    void Start()
    {
        if (this.GetComponent<SpriteRenderer>() != null && MapManager.Instance.map != null)
        {
            this.GetComponent<SpriteRenderer>().sortingOrder = MapManager.Instance.groundLayerID;
        }
    }

    void OnEnable()
    {
        Enemy.OnEnemyDamage += UpdateHpBar;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDamage -= UpdateHpBar;

    }
    void UpdateHpBar(int hpPercentage, AbstractCharacter character)
    {
        if (character == this.GetComponentInParent<AbstractCharacter>())
        {
            int animID = hpSprites.Length - (hpPercentage / 4);
            //Debug.Log(hpPercentage);
            this.GetComponent<SpriteRenderer>().sprite = hpSprites[animID];
        }
    }
}