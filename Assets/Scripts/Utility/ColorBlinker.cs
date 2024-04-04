using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBlinker : MonoBehaviour
{
    public void OnEnable()
    {
        StartCoroutine(blink(0.075f, 8, new Color(0.9f, 0f, 0f, 1f))); 
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