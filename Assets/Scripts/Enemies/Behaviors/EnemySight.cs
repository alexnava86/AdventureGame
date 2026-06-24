// =============================================================================
// EnemySight.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Gives an enemy a field of vision. Each FixedUpdate it checks whether the
// player is inside a vision cone (triangle) and not blocked by obstacles via
// raycast. Fires UnityEvents on first-sight, on continued sight, and on losing
// sight, so other components/designers can react without code changes.
//
// The vision cone is drawn in the Scene view (gizmos) for easy tuning, and
// can optionally face the enemy's movement/patrol direction automatically.
//
// SETUP:
//   • Assign the Player the "Player" tag (or set targetTag)
//   • Set obstacleLayer to your solid ground/wall layers
//   • Wire OnPlayerSpotted / OnPlayerLost in the Inspector to other behaviours
//     (e.g. enable an EnemyProjectile, switch Patrol → Chase, play an alert anim)
// =============================================================================

using UnityEngine;
using UnityEngine.Events;

public class EnemySight : MonoBehaviour
{
    [Header("Vision Cone")]
    [Tooltip("How far the enemy can see (px).")]
    public float viewDistance = 120f;
    [Tooltip("Total cone width in degrees (e.g. 60 = 30° either side of facing).")]
    [Range(1f, 360f)]
    public float viewAngle = 60f;
    [Tooltip("Eye position offset from the enemy centre (px).")]
    public Vector2 eyeOffset = new Vector2(0f, 8f);

    [Header("Facing")]
    [Tooltip("True = enemy faces right. Flip this from Patrol or movement code.")]
    public bool facingRight = false;
    [Tooltip("If true, facingRight is auto-derived from Rigidbody2D horizontal velocity.")]
    public bool autoFaceFromVelocity = true;

    [Header("Targeting")]
    [Tooltip("Tag of the object the enemy is looking for.")]
    public string targetTag = "Player";
    [Tooltip("Layers that block line of sight (walls, ground).")]
    public LayerMask obstacleLayer;
    [Tooltip("How often to run the sight check (seconds). 0 = every FixedUpdate.")]
    public float checkInterval = 0.1f;

    [Header("Events")]
    [Tooltip("Fired once, the frame the player first enters view.")]
    public UnityEvent OnPlayerSpotted;
    [Tooltip("Fired every check while the player remains in view.")]
    public UnityEvent OnPlayerInSight;
    [Tooltip("Fired once, the frame the player leaves view.")]
    public UnityEvent OnPlayerLost;

    [Header("Editor Gizmo")]
    public bool drawGizmo = true;
    public Color gizmoColorIdle    = new Color(1f, 1f, 0f, 0.15f);
    public Color gizmoColorSpotted = new Color(1f, 0f, 0f, 0.25f);

    // ── Runtime state ─────────────────────────────────────────────────────────
    public bool CanSeePlayer { get; private set; }
    private Transform   target;
    private Rigidbody2D rb;
    private float       timer;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        var t = GameObject.FindWithTag(targetTag);
        if (t != null) target = t.transform;
    }

    private void FixedUpdate()
    {
        if (checkInterval > 0f)
        {
            timer -= Time.fixedDeltaTime;
            if (timer > 0f) return;
            timer = checkInterval;
        }

        if (autoFaceFromVelocity && rb != null && Mathf.Abs(rb.velocity.x) > 0.1f)
            facingRight = rb.velocity.x > 0f;

        bool couldSee = CanSeePlayer;
        CanSeePlayer  = CheckSight();

        if (CanSeePlayer && !couldSee) OnPlayerSpotted?.Invoke();
        if (CanSeePlayer)              OnPlayerInSight?.Invoke();
        if (!CanSeePlayer && couldSee) OnPlayerLost?.Invoke();
    }

    private bool CheckSight()
    {
        if (target == null) return false;

        Vector2 eye   = EyeWorldPos();
        Vector2 toTgt = (Vector2)target.position - eye;
        float   dist  = toTgt.magnitude;

        // Distance gate
        if (dist > viewDistance) return false;

        // Angle gate — is the target within the cone?
        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;
        float   angle     = Vector2.Angle(facingDir, toTgt);
        if (angle > viewAngle * 0.5f) return false;

        // Line-of-sight gate — is anything solid between eye and target?
        RaycastHit2D hit = Physics2D.Raycast(eye, toTgt.normalized, dist, obstacleLayer);
        if (hit.collider != null) return false;   // blocked by a wall

        return true;
    }

    private Vector2 EyeWorldPos()
    {
        float signX = facingRight ? 1f : -1f;
        return (Vector2)transform.position + new Vector2(eyeOffset.x * signX, eyeOffset.y);
    }

    // =========================================================================
    // Editor visualisation
    // =========================================================================

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Vector2 eye       = EyeWorldPos();
        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;
        float   half      = viewAngle * 0.5f;

        // Cone edges
        Vector2 left  = RotateVec(facingDir,  half) * viewDistance;
        Vector2 right = RotateVec(facingDir, -half) * viewDistance;

        Gizmos.color = (Application.isPlaying && CanSeePlayer) ? gizmoColorSpotted : gizmoColorIdle;

        // Draw the cone as a fan of lines (cheap triangle approximation)
        const int segments = 12;
        Vector2 prev = eye + RotateVec(facingDir, half) * viewDistance;
        for (int i = 1; i <= segments; i++)
        {
            float t   = (float)i / segments;
            float ang = Mathf.Lerp(half, -half, t);
            Vector2 next = eye + RotateVec(facingDir, ang) * viewDistance;
            Gizmos.DrawLine(eye, next);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private static Vector2 RotateVec(Vector2 v, float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
