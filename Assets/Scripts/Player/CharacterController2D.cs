// =============================================================================
// CharacterController2D.cs   —   Assets/Scripts/Player/
//
// RIGIDBODY2D INSPECTOR SETTINGS:
//   Gravity Scale       : 0   (script applies gravity manually)
//   Mass                : 150
//   Linear Drag         : 0
//   Collision Detection : Continuous
//   Freeze Rotation Z   : ✓
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    // =========================================================================
    // Inspector
    // =========================================================================

    [Header("Movement & Jump  (original fields preserved)")]
    public LayerMask  ground;
    public Transform  groundCheck;
    public float      speed      = 96f;
    public float      jumpHeight = 320f;

    [Header("Jumps")]
    [Tooltip("Max jumps before landing. 1 = normal, 2 = double-jump powerup.")]
    [Min(1)]
    public int maxJumps = 1;

    [Header("Movement Feel")]
    [Tooltip("Ground acceleration (px/s²). Default 9999 = instant (original feel). " +
             "Terrain modifiers can lower this for ice/mud.")]
    public float groundAcceleration    = 9999f;
    [Tooltip("Air acceleration (px/s²).")]
    public float airAcceleration       = 9999f;
    [Tooltip("Multiplier on acceleration when decelerating (no input). Higher = snappier stops.")]
    public float decelerationMultiplier = 2.5f;

    [Header("Gravity Tuning — 1 PPU")]
    [Tooltip("Scales Physics2D.gravity.y. Use 100 at 1 PPU → effective -981 px/s².")]
    public float gravityMultiplier      = 100f;
    [Tooltip("Extra gravity while falling. 2.5 = punchy Mario arc. 1 = symmetric.")]
    public float fallGravityMultiplier  = 2.5f;
    [Tooltip("Extra gravity on early Jump release.")]
    public float lowJumpMultiplier      = 2.0f;

    [Header("Variable Jump & Feel")]
    [Range(0f, 1f)]
    [Tooltip("Upward-velocity fraction kept on early Jump release. 0.4 = Mario half-cut.")]
    public float jumpCutFraction = 0.4f;
    [Tooltip("Seconds after leaving a ledge that a jump is still allowed.")]
    public float coyoteTime      = 0.12f;
    [Tooltip("Seconds before landing that a Jump press fires on touch-down.")]
    public float jumpBufferTime  = 0.15f;
    [Tooltip("Terminal fall speed cap (px/s).")]
    public float maxFallSpeed    = 1200f;

    // =========================================================================
    // Private state
    // =========================================================================

    private PlayerBaseInput playerBaseInputs;
    private Rigidbody2D     rb;
    private Animator        animator;

    private float horizontal;
    private float vertical;
    private bool  jumpPressedThisFrame;
    private bool  jumpHeld;

    private bool  isGrounded;
    private bool  wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpGroundCooldown;
    private int   jumpsRemaining;
    private const float JUMP_COOLDOWN = 0.12f;

    // Animation state
    private enum State     { Idle, Walking, Ducking, Sneaking, Attacking, Jumping, Falling }
    private enum Direction { Left, Right }
    private State     state     = State.Idle;
    private Direction direction = Direction.Right;

    // Surface terrain (collision-based — TerrainType component)
    private TerrainModifiers activeTerrain;

    // Volume terrain (trigger-based — TerrainVolume component, e.g. water / quicksand)
    private TerrainModifiers activeVolume;
    private int   volumeCount = 0;      // tracks overlapping volumes
    private float lethalTimer = 0f;

    // ── Terrain-aware effective values ────────────────────────────────────────

    private float EffectiveSpeed =>
        speed * (activeTerrain?.speedMultiplier ?? 1f) * (activeVolume?.speedMultiplier ?? 1f);

    private float EffectiveJumpHeight =>
        jumpHeight * (activeTerrain?.jumpMultiplier ?? 1f) * (activeVolume?.jumpMultiplier ?? 1f);

    // Combined fall gravity: base × surface friction × volume floatiness
    private float EffectiveFallGravity =>
        fallGravityMultiplier
        * (activeTerrain?.fallGravityMultiplier ?? 1f)
        * (activeVolume?.fallGravityMultiplier  ?? 1f);

    // Volume-wide gravity scale — applied to ascent and apex too (water whole-arc floatiness)
    private float EffectiveVolumeGravScale => activeVolume?.volumeGravityScale ?? 1f;

    private float EffectiveGroundAccel =>
        (activeTerrain != null && activeTerrain.overrideAcceleration)
            ? activeTerrain.groundAcceleration
            : groundAcceleration;

    private float EffectiveDecelMult =>
        decelerationMultiplier * (activeTerrain?.frictionMultiplier ?? 1f);

    private bool JumpBlockedByTerrain =>
        (activeTerrain != null && activeTerrain.preventJump) ||
        (activeVolume  != null && activeVolume.preventJump);

    private bool InfiniteJumpInVolume =>
        activeVolume != null && activeVolume.infiniteJump && !activeVolume.preventJump;

    // =========================================================================
    // Public API
    // =========================================================================

    public Vector3 velocity  => rb != null ? (Vector3)rb.velocity : Vector3.zero;
    public bool    IsGrounded => isGrounded;
    public bool    IsInVolume => activeVolume != null;

    /// <summary>True when the player is facing right.</summary>
    public bool FacingRight => direction == Direction.Right;

    /// <summary>Raw vertical axis value (-1 down … +1 up).</summary>
    public float CurrentVerticalInput => vertical;

    /// <summary>Raw horizontal axis value (-1 left … +1 right).</summary>
    public float CurrentHorizontalInput => horizontal;

    /// <summary>
    /// Set to true by an ability (e.g. WhipAbility while swinging) to pause all
    /// CharacterController2D physics so the ability can drive the Rigidbody directly.
    /// Remember to restore to false when the ability finishes.
    /// </summary>
    public bool IsExternallyControlled { get; set; }

    // =========================================================================
    // MonoBehaviour
    // =========================================================================

    public void Awake()
    {
        playerBaseInputs = new PlayerBaseInput();
        playerBaseInputs.Overworld.Disable();
        playerBaseInputs.Character.Enable();

        animator       = GetComponent<Animator>();
        rb             = GetComponent<Rigidbody2D>();
        direction      = Direction.Right;
        state          = State.Idle;
        jumpsRemaining = maxJumps;
        rb.gravityScale = 0f;
    }

    private void OnDisable()
    {
        playerBaseInputs?.Character.Disable();
        playerBaseInputs?.Dispose();
    }

    private void FixedUpdate()
    {
        // An ability (e.g. WhipAbility while swinging) can pause all physics here
        if (IsExternallyControlled) return;

        // ── Timers ─────────────────────────────────────────────────────────
        if (jumpGroundCooldown > 0f) jumpGroundCooldown -= Time.fixedDeltaTime;
        if (jumpBufferTimer    > 0f) jumpBufferTimer    -= Time.fixedDeltaTime;

        // ── Ground detection ───────────────────────────────────────────────
        wasGrounded = isGrounded;
        isGrounded  = CheckGrounded();

        // ── Land ───────────────────────────────────────────────────────────
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;

            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer = 0f;
                ExecuteJump();
            }
            else
            {
                RestoreGroundAnimation();
            }
        }

        // ── Coyote ────────────────────────────────────────────────────────
        if (wasGrounded && !isGrounded && rb.velocity.y <= 0f)
            coyoteTimer = coyoteTime;
        else if (isGrounded)
            coyoteTimer = 0f;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        // ── Jump decision ─────────────────────────────────────────────────
        if (jumpPressedThisFrame)
        {
            jumpPressedThisFrame = false;
            bool useCoyote = coyoteTimer > 0f && !isGrounded && jumpsRemaining == maxJumps;

            if ((isGrounded || useCoyote || jumpsRemaining > 0 || InfiniteJumpInVolume) && !JumpBlockedByTerrain)
            {
                if (useCoyote) coyoteTimer = 0f;
                ExecuteJump();
            }
            else
            {
                jumpBufferTimer = jumpBufferTime;
            }
        }

        // ── Physics ───────────────────────────────────────────────────────
        ApplyGravity();
        ApplyHorizontalMovement();

        // ── Volume continuous downforce (quicksand sinking, etc.) ─────────
        if (activeVolume != null && activeVolume.continuousDownforce > 0f)
        {
            float newVY = rb.velocity.y - activeVolume.continuousDownforce * Time.fixedDeltaTime;
            newVY = Mathf.Max(newVY, -maxFallSpeed);
            rb.velocity = new Vector2(rb.velocity.x, newVY);
        }

        // ── Volume lethal timer ────────────────────────────────────────────
        if (activeVolume != null && activeVolume.isLethal)
        {
            lethalTimer -= Time.fixedDeltaTime;
            if (lethalTimer <= 0f)
            {
                lethalTimer = 0f;
                // Integrate with your AbstractCharacter death system.
                // Replace with your event or direct call as needed.
                GetComponent<AbstractCharacter>()?.TakeDamage(9999);
            }
        }

        // ── Apex: switch Jump → Fall animation ────────────────────────────
        if (!isGrounded && state == State.Jumping && rb.velocity.y <= 0f)
        {
            state = State.Falling;
            SetAnim(direction == Direction.Right ? "FallRight" : "FallLeft");
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 6f);
    }

    // =========================================================================
    // Terrain detection — surface (collision) and volume (trigger)
    // =========================================================================

    // Surface terrain: fires when the character stands ON a TerrainType object
    private void OnCollisionEnter2D(Collision2D col)
    {
        foreach (ContactPoint2D c in col.contacts)
        {
            if (c.normal.y > 0.5f)
            {
                activeTerrain = col.gameObject.GetComponent<TerrainType>()?.modifiers;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        activeTerrain = null;
    }

    // Volume terrain: fires when the character ENTERS a water / quicksand trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // GetComponent only searches the collider's own GameObject.
        // GetComponentInParent also checks up the hierarchy — needed when
        // the Tilemap (with TerrainVolume attached) is a child of a Grid parent
        // and the CompositeCollider2D fires as the triggering collider.
        var vol = other.GetComponent<TerrainVolume>()
               ?? other.GetComponentInParent<TerrainVolume>();

        if (vol == null || vol.modifiers == null) return;

        volumeCount++;
        activeVolume = vol.modifiers;

        if (activeVolume.isLethal)
            lethalTimer = activeVolume.lethalDelay;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var vol = other.GetComponent<TerrainVolume>()
               ?? other.GetComponentInParent<TerrainVolume>();

        if (vol == null || vol.modifiers == null) return;

        volumeCount = Mathf.Max(0, volumeCount - 1);
        if (volumeCount == 0)
        {
            activeVolume = null;
            lethalTimer  = 0f;          // reset death clock on exit
        }
    }

    // =========================================================================
    // Physics helpers
    // =========================================================================

    private void ApplyGravity()
    {
        if (isGrounded && rb.velocity.y <= 0f) return;

        float baseGravity = Physics2D.gravity.y * gravityMultiplier;
        float mult;

        if (rb.velocity.y < 0f)
        {
            // Falling — extra gravity, scaled by surface + volume
            mult = EffectiveFallGravity;
        }
        else if (rb.velocity.y > 0f && !jumpHeld)
        {
            // Ascending, button released — cut arc; also floaty in water
            mult = lowJumpMultiplier * EffectiveVolumeGravScale;
        }
        else
        {
            // Full ascent — still floaty in water at the apex
            mult = 1f * EffectiveVolumeGravScale;
        }

        float newVY = rb.velocity.y + baseGravity * mult * Time.fixedDeltaTime;
        newVY = Mathf.Max(newVY, -maxFallSpeed);
        rb.velocity = new Vector2(rb.velocity.x, newVY);
    }

    private void ApplyHorizontalMovement()
    {
        float targetVX  = horizontal * EffectiveSpeed;
        float accelBase = isGrounded ? EffectiveGroundAccel : airAcceleration;
        float accel     = Mathf.Abs(horizontal) > 0.01f
                          ? accelBase
                          : accelBase * EffectiveDecelMult;

        float newVX = Mathf.MoveTowards(rb.velocity.x, targetVX, accel * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newVX, rb.velocity.y);
    }

    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, EffectiveJumpHeight);
        // Don't consume a jump charge while swimming — infinite jump keeps the pool full
        if (!InfiniteJumpInVolume)
            jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);
        if (isGrounded) jumpGroundCooldown = JUMP_COOLDOWN;
        state = State.Jumping;
        SetAnim(direction == Direction.Right ? "JumpRight" : "JumpLeft");
    }

    private bool CheckGrounded()
    {
        if (jumpGroundCooldown > 0f) return false;
        if (groundCheck == null)     return false;
        return Physics2D.OverlapCircle(groundCheck.position, 6f, ground);
    }

    /// <summary>
    /// Drops the player through a one-way platform they're standing on.
    /// Both hand-placed platforms and the map's generated PLATFORM strips use
    /// the OneWayPlatform component, so a single path handles them all.
    /// Returns true if a drop was triggered.
    /// </summary>
    private bool TryDropThroughPlatform()
    {
        if (groundCheck == null) return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, 6f);
        foreach (var h in hits)
        {
            var platform = h.GetComponent<OneWayPlatform>() ?? h.GetComponentInParent<OneWayPlatform>();
            if (platform != null)
            {
                platform.DropThrough();
                return true;
            }
        }
        return false;
    }

    // =========================================================================
    // Landing animation restoration
    // =========================================================================

    private void RestoreGroundAnimation()
    {
        if (vertical < 0f && Mathf.Abs(horizontal) > 0.01f)
        {
            direction = horizontal > 0f ? Direction.Right : Direction.Left;
            state = State.Sneaking;
            SetAnim(direction == Direction.Right ? "SneakRight" : "SneakLeft");
        }
        else if (vertical < 0f)
        {
            state = State.Ducking;
            SetAnim(direction == Direction.Right ? "DuckRight" : "DuckLeft");
        }
        else if (Mathf.Abs(horizontal) > 0.01f)
        {
            direction = horizontal > 0f ? Direction.Right : Direction.Left;
            state = State.Walking;
            SetAnim(direction == Direction.Right ? "WalkRight" : "WalkLeft");
        }
        else
        {
            state = State.Idle;
            SetAnim(direction == Direction.Right ? "IdleRight" : "IdleLeft");
        }
    }

    // =========================================================================
    // Input callbacks
    // =========================================================================

    public void Movement(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        vertical   = context.ReadValue<Vector2>().y;

        if (state == State.Jumping || state == State.Falling || state == State.Attacking) return;

        if (vertical >= 0f)
        {
            if (horizontal > 0f) { SetAnim("WalkRight"); direction = Direction.Right; state = State.Walking; }
            if (horizontal < 0f) { SetAnim("WalkLeft");  direction = Direction.Left;  state = State.Walking; }
        }

        if (vertical < 0f && horizontal == 0f)
        {
            SetAnim(direction == Direction.Right ? "DuckRight" : "DuckLeft");
            state = State.Ducking;
        }

        if (vertical < 0f && horizontal < 0f) { SetAnim("SneakLeft");  direction = Direction.Left;  state = State.Sneaking; }
        if (vertical < 0f && horizontal > 0f) { SetAnim("SneakRight"); direction = Direction.Right; state = State.Sneaking; }

        if (context.canceled && direction != Direction.Right)
            { SetAnim("IdleLeft");  direction = Direction.Left;  state = State.Idle; }
        else if (context.canceled && direction != Direction.Left)
            { SetAnim("IdleRight"); direction = Direction.Right; state = State.Idle; }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Holding DOWN + Jump while standing on a one-way platform = drop through it
            if (vertical < -0.3f && TryDropThroughPlatform())
                return;   // consumed by drop-through; don't also jump

            jumpPressedThisFrame = true;
            jumpHeld = true;
        }
        if (context.canceled)
        {
            jumpHeld = false;
            if (rb.velocity.y > 0f)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutFraction);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (direction != Direction.Right)
                SetAnim(state == State.Ducking ? "DuckAttackLeft"  : "AttackLeft");
            else
                SetAnim(state == State.Ducking ? "DuckAttackRight" : "AttackRight");
        }
        if (context.canceled)
        {
            if (direction != Direction.Right)
            {
                if      (state == State.Ducking) SetAnim("DuckLeft");
                else if (state == State.Walking) SetAnim("WalkLeft");
                else                             SetAnim("IdleLeft");
            }
            else
            {
                if      (state == State.Ducking) SetAnim("DuckRight");
                else if (state == State.Walking) SetAnim("WalkRight");
                else                             SetAnim("IdleRight");
            }
        }
    }

    // =========================================================================
    // Utilities
    // =========================================================================

    public void SetAnim(string animName)
    {
        if (animator == null) return;
        IEnumerable<string> others = from s in animator.parameters
                                     where s.name != animName
                                     select s.name;
        foreach (string s in others) animator.SetBool(s, false);
        animator.SetBool(animName, true);
    }
}
