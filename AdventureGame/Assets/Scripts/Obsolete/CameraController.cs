using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

	private GameObject player;
	private Transform playerPos;
	Transform camPos;

	#region MonoBehaviour
	void Awake ()
	{
		camPos = transform;
		player = GameObject.FindGameObjectWithTag ("Player");
		playerPos = player.transform;
	}
	void Start ()
	{

	}

	void FixedUpdate ()
	{
		if (playerPos.position.x > camPos.position.x + 96)
		{
			MoveRight ();
		}

		if (playerPos.position.x < camPos.position.x - 96)
		{
			MoveLeft ();
		}

		if (playerPos.position.y > camPos.position.y + 72)
		{
			MoveUp ();
		}

		if (playerPos.position.y < camPos.position.y - 72)
		{
			MoveDown ();
		}
	}

	void LateUpdate ()
	{
		camPos.position = new Vector3 (Mathf.Round (GetComponent<Camera> ().transform.position.x), Mathf.Round (GetComponent<Camera> ().transform.position.y), -10f);
	}
	#endregion

	#region Methods
	public void MoveUp ()
	{
		camPos.position = new Vector2 (camPos.position.x, camPos.position.y + 1f);
	}
	public void MoveDown ()
	{
		camPos.position = new Vector2 (camPos.position.x, camPos.position.y - 1);
	}
	public void MoveRight ()
	{
		camPos.position = new Vector2 (camPos.position.x + 1f, camPos.position.y);

	}
	public void MoveLeft ()
	{
		camPos.position = new Vector2 (camPos.position.x - 1f, camPos.position.y);
	}
	#endregion
}
