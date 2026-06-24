// =============================================================================
// EnemyLedgeStop.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Stops a walking enemy from strolling off platform edges (and optionally from
// walking into walls). Casts a short ray downward just ahead of the enemy; if
// there's no ground there, it flips the enemy's facing/patrol direction.
// Pairs with Patrol.cs — assign the Patrol component so this can reverse it.
// =============================================================================

using UnityEngine;

public class EnemyLedgeStop : MonoBehaviour
{
    [Header("Ledge Detection")]
    [Tooltip("Horizontal distance ahead of the enemy to test for ground (px).")]
    public float lookAhead = 10f;
    [Tooltip("How far down to probe for ground (px).")]
    public float probeDepth = 20f;
    [Tooltip("Layer(s) considered solid ground.")]
    public LayerMask groundLayer;

    [Header("Wall Detection (optional)")]
    [Tooltip("Also turn around when bumping into a wall ahead.")]
    public bool detectWalls = true;
    [Tooltip("Horizontal probe distance for walls (px).")]
    public float wallCheckDistance = 8f;

    [Header("Facing")]
    [Tooltip("True if the enemy currently faces right. Synced with Rigidbody velocity.")]
    public bool facingRight = false;

    private Rigidbody2D rb;

    private void OnEnable() { rb = GetComponent<Rigidbody2D>(); }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Derive facing from current movement
        if (Mathf.Abs(rb.velocity.x) > 0.1f)
            facingRight = rb.velocity.x > 0f;

        float dir = facingRight ? 1f : -1f;

        // ── Ledge check — probe downward just ahead of the feet ──────────────
        Vector2 ahead = (Vector2)transform.position + new Vector2(lookAhead * dir, 0f);
        bool groundAhead = Physics2D.Raycast(ahead, Vector2.down, probeDepth, groundLayer);

        // ── Wall check — probe horizontally ahead ────────────────────────────
        bool wallAhead = false;
        if (detectWalls)
            wallAhead = Physics2D.Raycast(transform.position, new Vector2(dir, 0f),
                                          wallCheckDistance, groundLayer);

        // ── Reverse if at a ledge or wall ────────────────────────────────────
        if (!groundAhead || wallAhead)
            Reverse();
    }

    private void Reverse()
    {
        facingRight = !facingRight;
        rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);

        // Reverse a Patrol component if one is present so it stays in sync
        var patrol = GetComponent<Patrol>();
        if (patrol != null) patrol.SendMessage("ReverseDirection",
                                               SendMessageOptions.DontRequireReceiver);
    }

    private void OnDrawGizmosSelected()
    {
        float dir = facingRight ? 1f : -1f;
        Vector2 ahead = (Vector2)transform.position + new Vector2(lookAhead * dir, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(ahead, ahead + Vector2.down * probeDepth);
        if (detectWalls)
            Gizmos.DrawLine(transform.position,
                            (Vector2)transform.position + new Vector2(dir * wallCheckDistance, 0f));
    }
}
