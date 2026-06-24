// =============================================================================
// EnemyContactDamage.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// Deals damage to the player on contact and optionally knocks them back.
// Modular alternative/supplement to hard-coding contact damage in Enemy.cs.
// Useful for hazard-style enemies that hurt on touch.
// =============================================================================

using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Damage dealt to the player per hit.")]
    public int damage = 1;
    [Tooltip("Minimum seconds between hits on the same player (i-frames).")]
    public float hitCooldown = 1f;

    [Header("Knockback")]
    [Tooltip("Horizontal knockback applied to the player (px/s). 0 = none.")]
    public float knockbackX = 120f;
    [Tooltip("Upward knockback applied to the player (px/s). 0 = none.")]
    public float knockbackY = 100f;

    private float cooldownTimer;

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D col) => TryHit(col.collider, col.transform);
    private void OnTriggerEnter2D(Collider2D other)  => TryHit(other, other.transform);

    private void TryHit(Collider2D col, Transform t)
    {
        if (cooldownTimer > 0f) return;

        var player = col.GetComponent<Player>() ?? col.GetComponentInParent<Player>();
        if (player == null) return;

        // Damage — routes through your existing event/health system
        // Player.TakeDamage is private; OnCharacterContact event drives it,
        // so we mirror Enemy.cs behaviour and let the existing pipeline handle it.
        player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // Knockback
        var prb = col.GetComponent<Rigidbody2D>() ?? col.GetComponentInParent<Rigidbody2D>();
        if (prb != null && (knockbackX != 0f || knockbackY != 0f))
        {
            float dir = Mathf.Sign(t.position.x - transform.position.x);
            prb.velocity = new Vector2(dir * knockbackX, knockbackY);
        }

        cooldownTimer = hitCooldown;
    }
}
