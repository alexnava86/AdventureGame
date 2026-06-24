// =============================================================================
// EnemyJump.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Modular hop behaviour. Attach to any enemy with a Rigidbody2D and
// AbstractCharacter. The enemy periodically jumps while grounded.
// Pairs naturally with Patrol.cs for hopping patrollers.
// =============================================================================

using UnityEngine;

public class EnemyJump : MonoBehaviour
{
    [Header("Jump")]
    [Tooltip("Upward velocity applied on each jump (px/s).")]
    public float jumpHeight = 260f;

    [Tooltip("Seconds between jump attempts.")]
    public float jumpInterval = 2f;

    [Tooltip("Random +/- variation added to the interval so hops aren't robotic.")]
    public float intervalJitter = 0.5f;

    [Header("Ground Check")]
    [Tooltip("Empty child transform at the enemy's feet.")]
    public Transform groundCheck;
    [Tooltip("Radius of the ground overlap check (px).")]
    public float groundCheckRadius = 6f;
    [Tooltip("Layer(s) considered solid ground.")]
    public LayerMask groundLayer;

    [Header("Gravity (matches CharacterController2D scale)")]
    public float gravityMultiplier   = 100f;
    public float fallGravityMultiplier = 2.5f;

    private Rigidbody2D      rb;
    private AbstractCharacter character;
    private float           timer;
    private bool            isGrounded;

    private void OnEnable()
    {
        rb        = GetComponent<Rigidbody2D>();
        character = GetComponent<AbstractCharacter>();
        if (rb != null) rb.gravityScale = 0f;   // manual gravity for consistent feel
        timer = NextInterval();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Manual gravity
        if (!isGrounded || rb.velocity.y > 0f)
        {
            float g    = Physics2D.gravity.y * gravityMultiplier;
            float mult = rb.velocity.y < 0f ? fallGravityMultiplier : 1f;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + g * mult * Time.fixedDeltaTime);
        }

        // Jump timing
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            timer = NextInterval();
        }
    }

    private float NextInterval() => jumpInterval + Random.Range(-intervalJitter, intervalJitter);
}
