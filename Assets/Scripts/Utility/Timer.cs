using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TimeSpan time; //How long the timer is set for...
    public bool Started { get; set; }
    public bool Finished { get; set; }

    public void StartTimer(TimeSpan timeSpan)
    {
        Started = true;
        time = timeSpan;
        StartCoroutine(TimeStep());   
    }

    private IEnumerator TimeStep()//(TimeSpan timeSpan)
    {
        double totalTime = 0f; //Time incrementer, measured in seconds..

        while (time.TotalSeconds > totalTime)
        {
            totalTime += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        Finished = true;
        //Debug.Log("Timer completed in " + totalTime + " seconds");
        yield return null;
    }
}