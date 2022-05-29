using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class Player : MonoBehaviour
{
	#region Variables
	public int health;
	private bool grounded = true;
	private bool jumping = false;
	private bool jumpHeightReached = false;
	private bool running; // Run/ Attack/ Walk/ Idle
	private float speed = 1f;
	private float jumpHeight;
	private Rigidbody2D body;
	private Vector3 location;
	#endregion

	#region Properties
	#endregion

	#region MonoBehaviour
	void Start ()
	{
		body = this.GetComponent<Rigidbody2D> ();

	}

	void Update ()
	{
		if (Input.GetButtonDown ("Button1"))
		{

		}
		if (Input.GetButtonDown ("Button2") && jumping != true)
		{
			StartCoroutine (JumpRoutine ());
		}
		if (Input.GetButtonDown ("Button3"))
		{

		}
		if (Input.GetButtonDown ("Button4"))
		{

		}

		if (running != false)
		{
			speed = 2f;
			if (Input.GetAxis ("Horizontal") != 0)
			{
				if (Input.GetAxis ("Horizontal") > 0.5)
				{
					//runRight
				}
				if (Input.GetAxis ("Horizontal") < -0.5)
				{
					//runLeft
				}
			}
			if (Input.GetAxis ("Vertical") != 0)
			{
				if (Input.GetAxis ("Vertical") > 0.5)
				{
					//runUp
				}
				if (Input.GetAxis ("Vertical") < -0.5)
				{
					//runDown
				}
			}
		}
		else
		{
			/*
			speed = 1f;
			if (Input.GetAxis ("Horizontal") != 0)
			{
				if (Input.GetAxis ("Horizontal") > 0.5)
				{
					if (CharAnim.GetBool ("WalkRight") == false)
					{
						SetAnim ("WalkRight");
					}
				}
				else
				{
					if (CharAnim.GetBool ("WalkRight") == true)
					{
						SetAnim ("StandRight");
					}
				}

				if (Input.GetAxis ("Horizontal") < -0.5)
				{
					if (CharAnim.GetBool ("WalkLeft") == false)
					{
						SetAnim ("WalkLeft");
					}
				}
				else
				{
					if (CharAnim.GetBool ("WalkLeft") == true)
					{
						SetAnim ("StandLeft");
					}
				}
			}
			if (Input.GetAxis ("Vertical") != 0)
			{
				if (Input.GetAxis ("Vertical") > 0.5)
				{
					SetAnim ("WalkUp");
				}
				else
				{
					if (CharAnim.GetBool ("WalkUp") == true)
					{
						SetAnim ("StandBack");
					}
				}
				if (Input.GetAxis ("Vertical") < -0.5)
				{
					SetAnim ("WalkDown");
				}
				else
				{
					if (CharAnim.GetBool ("WalkDown") == true)
					{
						SetAnim ("StandFront");
					}
				}
			}
			*/
		}
	}

	public void FixedUpdate ()
	{
		if (Input.GetAxis ("Horizontal") != 0) //&& jumping != true)
		{
			location = new Vector2 (this.transform.position.x + speed * Input.GetAxis ("Horizontal"), this.transform.position.y);
			body.MovePosition (location);
		}
	}

	public void OnCollisionEnter2D (Collision2D collision)
	{
		//jumping = false;
		grounded = true;
	}
	#endregion

	#region Methods
	public void Jump ()
	{
		if (this.transform.position.y < jumpHeight)
		{
			jumping = true;
			location = new Vector2 (this.transform.position.x + speed * Input.GetAxis ("Horizontal"), this.transform.position.y + 16f);
			body.MovePosition (location);
			Debug.Log (location);
		}
		else
		{
			jumpHeightReached = true;
		}
	}
	public void Fall ()
	{
		jumping = true;
		location = new Vector2 (this.transform.position.x + speed * Input.GetAxis ("Horizontal"), this.transform.position.y - 16f);
		body.MovePosition (location);
		Debug.Log ("AAA");
	}
	#endregion

	#region Coroutines
	public IEnumerator JumpRoutine ()
	{
		float timer = 0;
		float jumpTime = 1f;
		Vector2 jumpVector = new Vector2 (this.transform.position.x, this.transform.position.y + 1024f);
		body.velocity = Vector2.zero;
		jumping = true;
		Debug.Log ("JUMPING...");
		while (jumping == true && timer < jumpTime)
		{
			//Calculate how far through the jump we are as a percentage
			//apply the full jump force on the first frame, then apply less force
			//each consecutive frame
			float proportionCompleted = timer / jumpTime;
			Vector2 thisFrameJumpVector = Vector2.Lerp (jumpVector, Vector2.zero, proportionCompleted);
			body.AddForce (thisFrameJumpVector);
			timer += Time.deltaTime;
			yield return null;
		}
		Debug.Log ("Done!");
		jumping = false;
	}
	#endregion
}
