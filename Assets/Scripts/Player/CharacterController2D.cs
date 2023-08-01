//using System;
using System.Linq;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using Prime31;

public class CharacterController2D : MonoBehaviour
{
    public LayerMask ground;
    public Transform groundCheck;
    public float speed = 96;
    public float jumpHeight = 320;
    public Vector3 velocity;

    private PlayerBaseInput playerBaseInputs;
    private Rigidbody2D rb;
    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool facingRight;
    private bool ducking;
    private enum State { Walking, Running, Idle, Ducking, Jumping, Attacking };
    private enum Direction { Left, Right };
    private State state;
    private Direction direction;

    /*
    private bool IsGrounded
    {
        get
        {
            return Physics2D.OverlapCircle(new Vector2(groundCheck.transform.position.x, this.transform.position.y - 0.5f), 0.2f, ground);
        }
    }
    */

    public void Awake()
    {
        playerBaseInputs = new PlayerBaseInput();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        direction = Direction.Right;
        state = State.Idle;
        //facingRight = true;

        // listen to some events for illustration purposes
        //_controller.onControllerCollidedEvent += onControllerCollider;
        //_controller.onTriggerEnterEvent += onTriggerEnterEvent;
        //_controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    private void Start()
    {

    }

    private void Update()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (!facingRight && horizontal > 0)
        {
            //FlipAxis();
        }
        if (facingRight && horizontal < 0)
        {
            //FlipAxis();
        }
    }

    private void LateUpdate()
    {
        this.transform.position = new Vector2(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y));
    }

    ///*
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 6f, ground); //(new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.5f), 0.2f, ground); //
    }
    //*/

    private void FlipAxis()
    {
        facingRight = !facingRight;
        Vector3 localScale = this.transform.localScale;
        localScale.x *= -1f;
        this.transform.localScale = localScale;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical = context.ReadValue<Vector2>().y;
        if (horizontal > 0f)
        {
            SetAnim("WalkRight");
            //facingRight = true;
            direction = Direction.Right;
            state = State.Walking;
        }
        else if (horizontal < 0f)
        {
            SetAnim("WalkLeft");
            //facingRight = false;
            direction = Direction.Left;
            state = State.Walking;
        }
        if (vertical < 0f && direction != Direction.Right) //facingRight != true)
        {
            SetAnim("DuckLeft");
            state = State.Ducking;
        }
        else if (vertical < 0f)
        {
            SetAnim("DuckRight");
            state = State.Ducking;
        }

        if (context.canceled && direction != Direction.Right) //facingRight != true)
        {
            SetAnim("IdleLeft");
            state = State.Idle;
        }
        else if (context.canceled && direction == Direction.Right) //facingRight == true)
        {
            SetAnim("IdleRight");
            state = State.Idle;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            if (direction != Direction.Right)
            {
                SetAnim("JumpLeft");
            }
            if (direction != Direction.Left)
            {
                SetAnim("JumpRight");
            }
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        Animator anim = this.GetComponent<Animator>();
        if (context.started && direction != Direction.Right) //facingRight != true)//(context.started)
        {
            if (state != State.Ducking)
            {
                SetAnim("AttackLeft");
            }
            else
            {
                SetAnim("DuckAttackLeft");
            }
        }
        else if (context.started && direction == Direction.Right) //facingRight == true)//(context.started)
        {
            if (state != State.Ducking)
            {
                SetAnim("AttackRight");
            }
            else
            {
                SetAnim("DuckAttackRight");
            }
        }

        if (context.canceled && direction != Direction.Right) //facingRight != true)
        {
            if (state != State.Ducking)
            {
                if (state != State.Walking)
                {
                    SetAnim("IdleLeft");
                }
                else
                {
                    SetAnim("WalkLeft");
                }
            }
            else
            {
                SetAnim("DuckLeft");
            }
        }
        if (context.canceled && direction == Direction.Right) //facingRight == true)
        {
            if (state != State.Ducking)
            {
                if (state != State.Walking)
                {
                    SetAnim("IdleRight");
                }
                else
                {
                    SetAnim("WalkRight");
                }
            }
            else
            {
                SetAnim("DuckRight");
            }
        }
    }

    public void SetAnim(string animName)
    {
        if (this.GetComponent<Animator>() != null)
        {
            Animator anim = this.GetComponent<Animator>();
            IEnumerable<string> state = from s in anim.parameters where s.name != animName select s.name;

            foreach (string s in state)
            {
                anim.SetBool(s, false);
            }
            anim.SetBool(animName, true);
        }
    }
    /*
    private PlayerBaseInput playerBaseInputs;
    private InputAction movement;
    private InputAction jump;

    public float gravity = -25f;
    public float runSpeed = 8f;
    public float groundDamping = 20f; // how fast do we change direction? higher means faster
    public float inAirDamping = 5f;
    public float jumpHeight = 3f;

    [HideInInspector]
    private float normalizedHorizontalSpeed = 0;

    private CharacterController2D _controller;
    private Animator _animator;
    private RaycastHit2D _lastControllerColliderHit;
    private Vector3 _velocity;

    #region MonoBehaviour

    public void Awake()
    {
        playerBaseInputs = new PlayerBaseInput();
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    private void Update()
    {

    }

    private void OnEnable()
    {
        movement = playerBaseInputs.Character.Movement;
        movement.Enable();

        jump = playerBaseInputs.Character.Jump;
        jump.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        jump.Disable();
    }

    public void OnMovement(InputValue input)
    {
        Vector2 inputVec = input.Get<Vector2>();
        Vector3 moveVec = new Vector3(inputVec.x, 0, inputVec.y);


        if (input.Get<Vector2>().x > 0f)
        {
            Debug.Log("Right");
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x < 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            //if (_controller.isGrounded)
            //_animator.Play( Animator.StringToHash( "Run" ) );
        }
        else if (input.Get<Vector2>().x < 0f)
        {
            Debug.Log("Left");
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x > 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            //if (_controller.isGrounded)
            //_animator.Play( Animator.StringToHash( "Run" ) );
        }
        else
        {
            normalizedHorizontalSpeed = 0;

            //if (_controller.isGrounded)
            //_animator.Play( Animator.StringToHash( "Idle" ) );
        }

        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);

        // apply gravity before moving
        _velocity.y += gravity * Time.deltaTime;

        // if holding down bump up our movement amount and turn off one way platform detection for a frame.
        // this lets us jump down through one way platforms
        if (_controller.isGrounded && input.Get<Vector2>().y < 0f)
        {
            _velocity.y *= 3f;
            _controller.ignoreOneWayPlatformsThisFrame = true;
        }

        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;
    }
    public void OnJump()
    {
        Debug.Log("HIT!");
        _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
        //_animator.Play( Animator.StringToHash( "Jump" ) );
    }

    #endregion

    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        //logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }

    void onTriggerEnterEvent(Collider2D col)
    {
        Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }

    void onTriggerExitEvent(Collider2D col)
    {
        Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }

    #endregion
    /*
            if (Input.GetKey(KeyCode.RightArrow))
            {
                normalizedHorizontalSpeed = 1;
                if (transform.localScale.x < 0f)
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

                //if (_controller.isGrounded)
                //_animator.Play( Animator.StringToHash( "Run" ) );
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                normalizedHorizontalSpeed = -1;
                if (transform.localScale.x > 0f)
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

                //if (_controller.isGrounded)
                //_animator.Play( Animator.StringToHash( "Run" ) );
            }
            else
            {
                normalizedHorizontalSpeed = 0;

                //if (_controller.isGrounded)
                //_animator.Play( Animator.StringToHash( "Idle" ) );
            }


            // we can only jump whilst grounded
            if (_controller.isGrounded && Input.GetButtonDown("Jump") == true) //Legacy code here, uses Up Arrow to Jump--> GetKeyDown(KeyCode.UpArrow))
            {
                _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
                //_animator.Play( Animator.StringToHash( "Jump" ) );
            }


            // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
            var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
            _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor);

            // apply gravity before moving
            _velocity.y += gravity * Time.deltaTime;

            // if holding down bump up our movement amount and turn off one way platform detection for a frame.
            // this lets us jump down through one way platforms
            if (_controller.isGrounded && Input.GetKey(KeyCode.DownArrow))
            {
                _velocity.y *= 3f;
                _controller.ignoreOneWayPlatformsThisFrame = true;
            }

            _controller.move(_velocity * Time.deltaTime);

            // grab our current _velocity to use as a base for all calculations
            _velocity = _controller.velocity;
            */
}