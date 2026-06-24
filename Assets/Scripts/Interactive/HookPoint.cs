// =============================================================================
// HookPoint.cs   —   Assets/Scripts/World/
//
// Place on any GameObject that should act as a swing anchor.
// Recommended setup:
//   • Sprite:   a 2–3 px square visual (gold/yellow works well)
//   • Collider: CircleCollider2D radius ~4px, Is Trigger = true
//   • Layer:    dedicated "HookPoint" layer
//
// The sprite pulses gently to draw the player's eye.  It brightens when the
// player's whip is aimed roughly toward it, so they can see it is reachable.
// =============================================================================

using UnityEngine;

public class HookPoint : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("How fast the idle pulse cycles (radians/second).")]
    public float pulseSpeed     = 2.5f;
    [Tooltip("Alpha range for the idle pulse (min, max).")]
    public float pulseAlphaMin  = 0.45f;
    public float pulseAlphaMax  = 0.95f;

    [Header("Interaction")]
    [Tooltip("Colour tint applied when the player's whip is actively aimed at this point.")]
    public Color highlightColor  = new Color(1f, 0.95f, 0.3f, 1f);
    [Tooltip("Default tint when idle.")]
    public Color defaultColor    = Color.white;

    // ── State set by WhipAbility ──────────────────────────────────────────────
    private bool           _highlighted;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_sr == null) return;

        if (_highlighted)
        {
            // Solid at full brightness when targeted
            _sr.color = highlightColor;
        }
        else
        {
            // Gentle sine-wave alpha pulse when idle
            float alpha = Mathf.Lerp(pulseAlphaMin, pulseAlphaMax,
                          (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            _sr.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, alpha);
        }
    }

    /// <summary>Called by WhipAbility to highlight this point when the whip is aimed at it.</summary>
    public void SetHighlighted(bool highlighted)
    {
        _highlighted = highlighted;
    }
}
