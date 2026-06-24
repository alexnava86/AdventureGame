// =============================================================================
// Arrow.cs   —   Assets/Scripts/Player/Abilities/
//
// Attach to an Arrow prefab.  Required components:
//   • SpriteRenderer  — assign a sprite (even a 4×1 px white line works for testing)
//   • Rigidbody2D     — Body Type: Dynamic, Gravity Scale: 0, Collision Detection: Continuous
//   • Collider2D      — small trigger (BoxCollider2D ~4×1 px)  IsTrigger = true
//
// BowAbility spawns instances of this prefab and calls Launch().
// =============================================================================

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Arrow : MonoBehaviour
{
    [Header("Physics")]
    [Tooltip("Gravity applied to the arrow each second (px/s²). " +
             "Should match your game's effective gravity (gravityMultiplier × 9.81).")]
    public float arrowGravity = 600f;

    [Tooltip("When false, the arrow flies in a straight line with no gravity (horizontal shots). " +
             "Set true by BowAbility only for diagonal shots so they arc.")]
    public bool affectedByGravity = false;

    [Tooltip("Seconds before the arrow destroys itself if it hasn't hit anything.")]
    public float lifetime     = 3f;

    [Header("Damage")]
    public int damage = 1;

    // ── Private ───────────────────────────────────────────────────────────────
    private Rigidbody2D  _rb;
    private float        _timer;
    private bool         _launched;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;  // we apply gravity manually for full control
    }

    private void Update()
    {
        if (!_launched) return;

        // Lifetime check
        _timer += Time.deltaTime;
        if (_timer >= lifetime) { Destroy(gameObject); return; }

        // Rotate sprite to face current velocity direction (gives arc illusion)
        if (_rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void FixedUpdate()
    {
        if (!_launched) return;
        if (!affectedByGravity) return;   // straight shots keep constant velocity

        // Apply downward gravity to create an arc
        _rb.velocity = new Vector2(_rb.velocity.x,
                                   _rb.velocity.y - arrowGravity * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_launched) return;

        // Don't collide with the player who fired this
        if (other.GetComponent<CharacterController2D>() != null) return;
        if (other.GetComponent<Player>() != null) return;

        // Deal damage to anything that takes it
        var target = other.GetComponent<AbstractCharacter>()
                  ?? other.GetComponentInParent<AbstractCharacter>();
        target?.TakeDamage(damage);

        // Stick to solid surfaces (non-trigger hits) — or just destroy
        if (!other.isTrigger)
            Destroy(gameObject);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>Fire the arrow in the given world-space direction at arrowSpeed px/s.</summary>
    public void Launch(Vector2 direction, float arrowSpeed)
    {
        _rb.velocity = direction.normalized * arrowSpeed;
        _launched    = true;
        _timer       = 0f;
    }
}
