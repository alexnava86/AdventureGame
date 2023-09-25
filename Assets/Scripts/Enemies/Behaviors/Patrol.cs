using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    [SerializeField]
    private float speed = 0f;
    [SerializeField]
    private int minutes;
    [SerializeField]
    private int seconds;
    private float horizontal;
    private float vertical;
    private bool walkingLeft = false;
    private bool walkingRight = false;
    private bool holdPosition = false;
    private AbstractCharacter character;
    private Rigidbody2D rb;
    //private Timer timer;

    private void OnEnable()
    {
        //Timer timer = this.gameObject.AddComponent<Timer>();
        //timer.StartTimer(new TimeSpan(0, 0, minutes, seconds, 0));
        character = GetComponent<AbstractCharacter>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(PatrolRight(new TimeSpan(0, 0, minutes, seconds, 0)));
    }
    private void Update()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }
    private void LateUpdate()
    {
        //this.transform.position = new Vector2(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y));
    }

    private IEnumerator PatrolLeft(TimeSpan timeSpan)
    {
        walkingLeft = true;
        horizontal = -1f;
        Timer timer = this.gameObject.AddComponent<Timer>();

        timer.StartTimer(timeSpan); //Rounds to the nearest 1/100 second (instead of 1/1000, possible framerate limitation)
        character.SetAnim("WalkLeft");
        
        while (timer.Finished != true)
        {
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(timer);
        //Debug.Log("PatrolLeft Complete");
        StartCoroutine(HoldPosition(timeSpan));
        yield return null;
    }
    private IEnumerator PatrolRight(TimeSpan timeSpan)
    {
        walkingRight = true;
        horizontal = 1f;
        Timer timer = this.gameObject.AddComponent<Timer>();

        timer.StartTimer(new TimeSpan(0, 0, minutes, seconds, 0)); //Rounds to the nearest 1/100 second (instead of 1/1000, framerate limitation)
        character.SetAnim("WalkRight");
        while (timer.Finished != true)
        {
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(timer);
        //Debug.Log("PatrolRight Complete");
        StartCoroutine(HoldPosition(timeSpan));
        yield return null;
    }
    private IEnumerator HoldPosition(TimeSpan timeSpan)
    {
        holdPosition = true;
        horizontal = 0f;
        Timer timer = this.gameObject.AddComponent<Timer>();

        if (walkingLeft != false)
        {
            character.SetAnim("IdleLeft");
        }
        else if (walkingRight != false)
        {
            character.SetAnim("IdleRight");
        }
        timer.StartTimer(new TimeSpan(0, 0, minutes, seconds, 0)); //Rounds to the nearest 1/100 second (instead of 1/1000, framerate limitation)
        while (timer.Finished != true)
        {
            yield return new WaitForSeconds(0.01f);
        }
        holdPosition = false;
        Destroy(timer);
        //Debug.Log("HoldPosition Complete");
        if (walkingLeft != false)
        {
            walkingLeft = false;
            StartCoroutine(PatrolRight(timeSpan));
        }
        else if (walkingRight != false)
        {
            walkingRight = false;
            StartCoroutine(PatrolLeft(timeSpan));
        }
        yield return null;
    }
}