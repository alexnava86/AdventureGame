// =============================================================================
// EnemyProjectile.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Modular ranged-attack behaviour. Attach to an enemy and assign a projectile
// prefab (anything with the Arrow.cs component, or your own projectile script).
// The enemy fires on an interval, aiming at the player if one is in range,
// otherwise firing in its facing direction.
//
// Works like the player's BowAbility but autonomous.
// =============================================================================

using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [Tooltip("Prefab to fire. An Arrow.cs component is ideal but any prefab works.")]
    public GameObject projectilePrefab;
    [Tooltip("Launch speed (px/s).")]
    public float projectileSpeed = 220f;
    [Tooltip("Spawn offset from enemy centre (px).")]
    public Vector2 spawnOffset = new Vector2(8f, 8f);

    [Header("Firing")]
    [Tooltip("Seconds between shots.")]
    public float fireInterval = 2.5f;
    [Tooltip("Random +/- variation on the interval.")]
    public float intervalJitter = 0.5f;

    [Header("Targeting")]
    [Tooltip("Max distance to detect and aim at the player (px). 0 = always fire forward.")]
    public float aimRange = 200f;
    [Tooltip("Make diagonal/aimed shots arc under gravity (only if prefab is an Arrow).")]
    public bool arcAtPlayer = true;
    [Tooltip("True = enemy faces/fires right by default; flipped by Patrol if present.")]
    public bool facingRight = false;

    private float timer;
    private Transform player;

    private void OnEnable()
    {
        timer = NextInterval();
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Fire();
            timer = NextInterval();
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null) return;

        Vector2 dir;
        bool    aimed = false;

        // Aim at player if in range
        if (player != null && aimRange > 0f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= aimRange)
            {
                dir   = ((Vector2)player.position - (Vector2)transform.position).normalized;
                aimed = true;
            }
            else dir = facingRight ? Vector2.right : Vector2.left;
        }
        else dir = facingRight ? Vector2.right : Vector2.left;

        float signX = dir.x >= 0f ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position
                       + new Vector2(spawnOffset.x * signX, spawnOffset.y);

        var go = Instantiate(projectilePrefab, origin, Quaternion.identity);

        // If the prefab is an Arrow, drive it through its Launch API
        var arrow = go.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.affectedByGravity = aimed && arcAtPlayer;
            arrow.Launch(dir, projectileSpeed);
        }
        else
        {
            // Generic fallback: push any Rigidbody2D the prefab has
            var prb = go.GetComponent<Rigidbody2D>();
            if (prb != null) prb.velocity = dir * projectileSpeed;
        }
    }

    private float NextInterval() => fireInterval + Random.Range(-intervalJitter, intervalJitter);
}
