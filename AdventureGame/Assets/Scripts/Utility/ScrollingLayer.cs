using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScrollingLayer : MonoBehaviour
{
    public enum Direction { North, East, South, West, NorthEast, NorthWest, SouthEast, SouthWest };
    public Direction direction;
    public int scrollSpeed;
    public float bgSize;
    private Vector3 startPos;

    void Start()
    {
        startPos = this.transform.position;
    }

    private void Update()
    {
        float newPos = Mathf.Repeat(Time.time * scrollSpeed, bgSize);
        switch (direction)
        {
            case Direction.West:
                transform.position = startPos + Vector3.left * newPos;
                break;
            case Direction.East:
                transform.position = startPos + Vector3.right * newPos;
                break;
            case Direction.North:
                transform.position = startPos + Vector3.up * newPos;
                break;
            case Direction.South:
                transform.position = startPos + Vector3.down * newPos;
                break;
            case Direction.NorthWest:
                transform.position = startPos + Vector3.up * newPos + Vector3.left * newPos;
                break;
            case Direction.NorthEast:
                transform.position = startPos + Vector3.up * newPos + Vector3.right * newPos;
                break;
            case Direction.SouthWest:
                transform.position = startPos + Vector3.down * newPos + Vector3.left * newPos;
                break;
            case Direction.SouthEast:
                transform.position = startPos + Vector3.down * newPos + Vector3.right * newPos;
                break;
        }
    }
}
