// =============================================================================
// EnemySwing.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Modular pendulum behaviour — an enemy that hangs from a fixed pivot and
// swings back and forth (think a spiked ball, a hanging bat, a trap).
// Mirrors the player's WhipAbility swing physics but stays permanently
// anchored to a pivot rather than latching on demand.
//
// Setup:
//   • Place the enemy GameObject at the pivot point in the scene
//   • The enemy swings on a rope of length 'ropeLength' below that pivot
//   • Rigidbody2D should be Kinematic (we drive position via the pendulum)
// =============================================================================

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemySwing : MonoBehaviour
{
    [Header("Pendulum")]
    [Tooltip("Rope length from the pivot to the enemy (px).")]
    public float ropeLength = 48f;
    [Tooltip("Starting angle from vertical (degrees). Positive = right.")]
    public float startAngleDeg = 60f;
    [Tooltip("Gravity strength driving the swing (px/s²).")]
    public float gravity = 600f;
    [Tooltip("Damping per second (0 = never slows, 0.1 = gradually settles).")]
    public float damping = 0f;

    [Header("Visuals")]
    [Tooltip("Optional LineRenderer to draw the rope.")]
    public LineRenderer ropeLine;

    private Vector2 pivot;
    private float   angle;       // radians from vertical
    private float   angularVel;

    private void Start()
    {
        // The enemy's start position IS the pivot; it hangs below.
        pivot  = transform.position;
        angle  = startAngleDeg * Mathf.Deg2Rad;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (ropeLine != null)
        {
            ropeLine.positionCount = 2;
            ropeLine.useWorldSpace = true;
        }
        UpdatePosition();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Pendulum: θ'' = -(g/L) sin(θ)
        angularVel += -(gravity / ropeLength) * Mathf.Sin(angle) * dt;
        angularVel *= Mathf.Pow(1f - damping, dt);
        angle      += angularVel * dt;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 pos = pivot + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * ropeLength;
        transform.position = pos;

        if (ropeLine != null)
        {
            ropeLine.SetPosition(0, pivot);
            ropeLine.SetPosition(1, pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualise the pivot and arc in the editor
        Vector2 p = Application.isPlaying ? pivot : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, 2f);
        Gizmos.DrawLine(p, p + Vector2.down * ropeLength);
    }
}
