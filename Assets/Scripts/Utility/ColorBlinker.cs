using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBlinker : MonoBehaviour
{
    private Color32 damageColor = new Color(255, 0, 0, 255); //Default red 'damage' blinking color

    public void changeBlinkColor()
    {
        Color32 newColor = new Color32(0, 0, 255, 255);
        damageColor = newColor;
    }

    public void OnEnable()
    {
        //TEST
        //changeBlinkColor();
        StartCoroutine(blink(0.075f, 8, damageColor)); 
    }

    IEnumerator blink(float delayBetweenBlinks, int numberOfBlinks, Color blinkColor)
    {
        var material = GetComponent<SpriteRenderer>().material;
        var counter = 0;
        while (counter <= numberOfBlinks)
        {
            material.SetColor("_BlinkColor", blinkColor);
            counter++;
            blinkColor.a = blinkColor.a == 1f ? 0f : 1f;
            yield return new WaitForSeconds(delayBetweenBlinks);
        }

        // revert to our standard sprite color
        blinkColor.a = 0f;
        material.SetColor("_BlinkColor", new Color(1f, 1f, 1f, 0f));
        this.enabled = false;
        //Debug.Log("Reached");
    }

    IEnumerator blinkSmooth(float timeScale, float duration, Color blinkColor)
    {
        var material = GetComponent<SpriteRenderer>().material;
        var elapsedTime = 0f;
        while (elapsedTime <= duration)
        {
            material.SetColor("_BlinkColor", blinkColor);

            blinkColor.a = Mathf.PingPong(elapsedTime * timeScale, 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // revert to our standard sprite color
        blinkColor.a = 0f;
        material.SetColor("_BlinkColor", new Color(1f, 1f, 1f, 0f));
        this.enabled = false;
    }
}