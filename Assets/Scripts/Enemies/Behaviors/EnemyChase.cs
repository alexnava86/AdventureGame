// =============================================================================
// EnemyChase.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Moves the enemy toward the player while active. Designed to be toggled on by
// EnemySight.OnPlayerSpotted and off by EnemySight.OnPlayerLost — wire those
// events to this component's BeginChase() / EndChase() in the Inspector.
//
// Starts DISABLED by default so the enemy patrols/idles until it spots the player.
// =============================================================================

using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    [Tooltip("Move speed while chasing (px/s).")]
    public float chaseSpeed = 70f;
    [Tooltip("Stop this close to the player so the enemy doesn't shove into them (px).")]
    public float stopDistance = 12f;
    [Tooltip("Keep chasing for this long after losing sight (seconds) before giving up.")]
    public float persistence = 1.5f;

    [Header("Optional Components")]
    [Tooltip("Patrol component to disable while chasing and re-enable when done.")]
    public Patrol patrolToPause;

    private Transform       target;
    private Rigidbody2D     rb;
    private AbstractCharacter character;
    private bool            chasing;
    private float           loseTimer;

    private void OnEnable()
    {
        rb        = GetComponent<Rigidbody2D>();
        character = GetComponent<AbstractCharacter>();
        var t = GameObject.FindWithTag("Player");
        if (t != null) target = t.transform;
    }

    /// <summary>Wire EnemySight.OnPlayerSpotted → this.</summary>
    public void BeginChase()
    {
        chasing   = true;
        loseTimer = 0f;
        if (patrolToPause != null) patrolToPause.enabled = false;
    }

    /// <summary>Wire EnemySight.OnPlayerLost → this. Starts the give-up countdown.</summary>
    public void EndChase()
    {
        loseTimer = persistence;   // keep chasing briefly, then stop
    }

    private void FixedUpdate()
    {
        if (!chasing) return;

        // Count down toward giving up once sight was lost
        if (loseTimer > 0f)
        {
            loseTimer -= Time.fixedDeltaTime;
            if (loseTimer <= 0f) { StopChasing(); return; }
        }

        if (target == null) return;

        float dx   = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (dist > stopDistance)
        {
            float dir = Mathf.Sign(dx);
            rb.velocity = new Vector2(dir * chaseSpeed, rb.velocity.y);
            character?.SetAnim(dir > 0f ? "WalkRight" : "WalkLeft");
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            character?.SetAnim(dx > 0f ? "IdleRight" : "IdleLeft");
        }
    }

    private void StopChasing()
    {
        chasing = false;
        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        if (patrolToPause != null) patrolToPause.enabled = true;
    }
}
