// =============================================================================
// AbstractAbility.cs   —   Assets/Scripts/Player/Abilities/
//
// Base class for every player ability (Whip, Bow, future items).
// Add concrete ability components to the player at runtime when the
// corresponding item is acquired:
//
//   GetComponent<PlayerAbilityManager>().AddAbility<WhipAbility>();
//
// Remove them when the item is lost:
//
//   GetComponent<PlayerAbilityManager>().RemoveAbility<WhipAbility>();
// =============================================================================

using UnityEngine;

public abstract class AbstractAbility : MonoBehaviour
{
    // ── Cached references (safe to use in any subclass) ───────────────────────
    protected CharacterController2D Controller { get; private set; }
    protected Rigidbody2D            RB         { get; private set; }
    protected Animator               Anim       { get; private set; }

    // ── Identity ──────────────────────────────────────────────────────────────
    /// <summary>Human-readable ability name, used for save data and UI labels.</summary>
    public abstract string AbilityName { get; }

    /// <summary>False when a cooldown or condition prevents use right now.</summary>
    public virtual bool CanUse => true;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        Controller = GetComponent<CharacterController2D>();
        RB         = GetComponent<Rigidbody2D>();
        Anim       = GetComponent<Animator>();
    }

    // ── Optional overrides ────────────────────────────────────────────────────
    /// <summary>Called once when the button is first pressed.</summary>
    public virtual void OnAbilityStarted()   { }

    /// <summary>Called every frame the button is held.</summary>
    public virtual void OnAbilityHeld()      { }

    /// <summary>Called when the button is released.</summary>
    public virtual void OnAbilityReleased()  { }
}
