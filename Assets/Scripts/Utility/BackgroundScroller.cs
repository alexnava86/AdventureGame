using UnityEngine;
using System.Collections;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed;
    public float bgSize;
    private Vector3 startPos;
    private GameObject leftCopy;
    private GameObject rightCopy;
    private GameObject upperCopy;
    private GameObject upperLeftCopy;
    private GameObject upperRightCopy;
    private GameObject lowerCopy;
    private GameObject lowerLeftCopy;
    private GameObject lowerRightCopy;
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        UpwardLeft,
        UpwardRight,
        DownwardLeft,
        DownwardRight
    };
    public Direction direction = Direction.Left;

    private void Start()
    {
        startPos = this.transform.position;
        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        Debug.Log(sr.bounds.min.x);
    }

    private void Update()
    {
        float newPos = Mathf.Repeat(Time.time * scrollSpeed, bgSize);
        switch (direction)
        {
            case Direction.Left:
                transform.position = startPos + Vector3.left * newPos;
                break;
            case Direction.Right:
                transform.position = startPos + Vector3.right * newPos;
                break;
            case Direction.Up:
                transform.position = startPos + Vector3.up * newPos;
                break;
            case Direction.Down:
                transform.position = startPos + Vector3.down * newPos;
                break;
            case Direction.UpwardLeft:
                transform.position = startPos + Vector3.up * newPos + Vector3.left * newPos;
                break;
            case Direction.UpwardRight:
                transform.position = startPos + Vector3.up * newPos + Vector3.right * newPos;
                break;
            case Direction.DownwardLeft:
                transform.position = startPos + Vector3.down * newPos + Vector3.left * newPos;
                break;
            case Direction.DownwardRight:
                transform.position = startPos + Vector3.down * newPos + Vector3.right * newPos;
                break;
        }
    }
}