using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCalculator : MonoBehaviour
{
    public Text scoreValueText;
    private int scoreValue;

    private void OnDestroy()
    {
        scoreValue = Convert.ToInt32(scoreValueText.text.ToString());
        scoreValue++;
        scoreValueText.text = String.Format("{0:000}", scoreValue);
    }
 }