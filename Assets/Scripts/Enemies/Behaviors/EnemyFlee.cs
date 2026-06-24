// =============================================================================
// EnemyFlee.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// The opposite of EnemyChase — the enemy runs AWAY from the player when active.
// Designed for timid creatures. Wire EnemySight.OnPlayerSpotted → BeginFlee
// and EnemySight.OnPlayerLost → EndFlee.
//
// Starts DISABLED behaviour (idle) until told to flee.
// =============================================================================

using UnityEngine;

public class EnemyFlee : MonoBehaviour
{
    [Header("Flee")]
    [Tooltip("Move speed while fleeing (px/s).")]
    public float fleeSpeed = 90f;
    [Tooltip("Keep fleeing this long after losing sight of the player (seconds).")]
    public float persistence = 2f;
    [Tooltip("If the player is farther than this, stop fleeing even if still seen (px). 0 = ignore.")]
    public float safeDistance = 160f;

    [Header("Optional Components")]
    [Tooltip("Patrol component to pause while fleeing and resume afterwards.")]
    public Patrol patrolToPause;

    private Transform        target;
    private Rigidbody2D      rb;
    private AbstractCharacter character;
    private bool             fleeing;
    private float            loseTimer;

    private void OnEnable()
    {
        rb        = GetComponent<Rigidbody2D>();
        character = GetComponent<AbstractCharacter>();
        var t = GameObject.FindWithTag("Player");
        if (t != null) target = t.transform;
    }

    /// <summary>Wire EnemySight.OnPlayerSpotted → this.</summary>
    public void BeginFlee()
    {
        fleeing   = true;
        loseTimer = 0f;
        if (patrolToPause != null) patrolToPause.enabled = false;
    }

    /// <summary>Wire EnemySight.OnPlayerLost → this.</summary>
    public void EndFlee() { loseTimer = persistence; }

    private void FixedUpdate()
    {
        if (!fleeing || target == null) return;

        if (loseTimer > 0f)
        {
            loseTimer -= Time.fixedDeltaTime;
            if (loseTimer <= 0f) { StopFleeing(); return; }
        }

        float dx   = transform.position.x - target.position.x; // away from player
        float dist = Mathf.Abs(target.position.x - transform.position.x);

        // Far enough away — relax
        if (safeDistance > 0f && dist >= safeDistance)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            character?.SetAnim(dx > 0f ? "IdleRight" : "IdleLeft");
            return;
        }

        float dir = Mathf.Sign(dx);
        rb.velocity = new Vector2(dir * fleeSpeed, rb.velocity.y);
        character?.SetAnim(dir > 0f ? "WalkRight" : "WalkLeft");
    }

    private void StopFleeing()
    {
        fleeing = false;
        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        if (patrolToPause != null) patrolToPause.enabled = true;
    }
}
