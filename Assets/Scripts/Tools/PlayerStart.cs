using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStart : MonoBehaviour
{
    public static PlayerStart Instance { get; private set; }
    public GameObject player { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector2(this.transform.position.x, this.transform.position.y - 8f);
        Camera.main.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + 48f);
    }
}
