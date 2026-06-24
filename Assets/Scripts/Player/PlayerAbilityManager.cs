// =============================================================================
// PlayerAbilityManager.cs   —   Assets/Scripts/Player/
//
// Attach to the Player GameObject alongside CharacterController2D.
//
// Wire the Ability1 and Ability2 actions in your PlayerInput component to
// the OnAbility1 / OnAbility2 methods here, exactly as you wired Movement,
// Jump, and Attack to CharacterController2D.
//
// Ability slots:
//   Ability1 — primary item ability  (e.g. Whip)
//   Ability2 — secondary item ability (e.g. Bow)
//
// To give the player an ability when they pick up an item:
//   GetComponent<PlayerAbilityManager>().AddAbility<WhipAbility>();
//
// To remove it:
//   GetComponent<PlayerAbilityManager>().RemoveAbility<WhipAbility>();
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityManager : MonoBehaviour
{
    // The two input slots — set to the currently equipped ability component.
    // Left null when the slot is empty (no item in that slot).
    private AbstractAbility _slot1;   // Ability1 button
    private AbstractAbility _slot2;   // Ability2 button

    // All abilities currently attached to this player
    private readonly List<AbstractAbility> _all = new List<AbstractAbility>();

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Auto-discover any AbstractAbility components already present on this
        // GameObject (i.e. added directly in the Inspector rather than via code).
        // This means you can add WhipAbility / BowAbility as components and they
        // will be automatically assigned to slots without any extra scripting.
        foreach (var ability in GetComponents<AbstractAbility>())
        {
            if (!_all.Contains(ability))
            {
                _all.Add(ability);
                AutoAssignSlot(ability);
                Debug.Log($"[PlayerAbilityManager] Auto-registered '{ability.AbilityName}' " +
                          $"→ slot {(_slot1 == ability ? 1 : 2)}");
            }
        }
    }

    // =========================================================================
    // Ability management API
    // =========================================================================

    /// <summary>Add a new ability component and auto-assign it to an empty slot.</summary>
    public T AddAbility<T>() where T : AbstractAbility
    {
        if (GetAbility<T>() != null) return GetAbility<T>(); // already has it
        var a = gameObject.AddComponent<T>();
        _all.Add(a);
        AutoAssignSlot(a);
        Debug.Log($"[PlayerAbilityManager] Added ability: {a.AbilityName}");
        return a;
    }

    /// <summary>Remove an ability component and clear its slot.</summary>
    public void RemoveAbility<T>() where T : AbstractAbility
    {
        var a = GetAbility<T>();
        if (a == null) return;
        _all.Remove(a);
        if (_slot1 == a) _slot1 = null;
        if (_slot2 == a) _slot2 = null;
        Destroy(a);
        Debug.Log($"[PlayerAbilityManager] Removed ability: {typeof(T).Name}");
    }

    /// <summary>Returns the first ability of the given type, or null.</summary>
    public T GetAbility<T>() where T : AbstractAbility
    {
        return _all.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Manually assign which ability responds to Ability1 or Ability2 button.
    /// slot = 1 or 2.
    /// </summary>
    public void AssignSlot(AbstractAbility ability, int slot)
    {
        if (slot == 1) _slot1 = ability;
        if (slot == 2) _slot2 = ability;
    }

    // =========================================================================
    // Input callbacks — wire these to your PlayerInput component
    // =========================================================================

    /// <summary>Wire to Ability1 action in the PlayerInput component (SendMessages or UnityEvents).</summary>
    public void Ability1(InputAction.CallbackContext context)
    {
        Debug.Log($"[PlayerAbilityManager] Ability1 fired — phase={context.phase}, " +
                  $"slot1={(_slot1 != null ? _slot1.AbilityName : "NULL")}, " +
                  $"canUse={(_slot1 != null && _slot1.CanUse)}");

        if (_slot1 == null || !_slot1.CanUse) return;
        if (context.started)   _slot1.OnAbilityStarted();
        if (context.performed) _slot1.OnAbilityHeld();
        if (context.canceled)  _slot1.OnAbilityReleased();
    }

    /// <summary>Wire to Ability2 action in the PlayerInput component.</summary>
    public void Ability2(InputAction.CallbackContext context)
    {
        Debug.Log($"[PlayerAbilityManager] Ability2 fired — phase={context.phase}, " +
                  $"slot2={(_slot2 != null ? _slot2.AbilityName : "NULL")}, " +
                  $"canUse={(_slot2 != null && _slot2.CanUse)}");

        if (_slot2 == null || !_slot2.CanUse) return;
        if (context.started)   _slot2.OnAbilityStarted();
        if (context.performed) _slot2.OnAbilityHeld();
        if (context.canceled)  _slot2.OnAbilityReleased();
    }

    // =========================================================================
    // Internal helpers
    // =========================================================================

    private void AutoAssignSlot(AbstractAbility a)
    {
        if (_slot1 == null)      { _slot1 = a; return; }
        if (_slot2 == null)      { _slot2 = a; }
    }
}
