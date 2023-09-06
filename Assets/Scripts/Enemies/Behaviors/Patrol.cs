using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [SerializeField]
    private float time { get; set; }
    private void OnEnable()
    {
        //StartCoroutine(blink(0.075f, 8, new Color(0.9f, 0f, 0f, 1f)));
    }
    private IEnumerator Move()
    {
        while (time > 0f)
        {
            
            yield return new WaitForSeconds(time);
        }
        yield return null;
    }
}
