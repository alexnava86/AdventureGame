// =============================================================================
// PlayerSword.cs
// Place in: Assets/Scripts/Items/Weapons/
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public int swordOffense;

    public delegate void PlayerAction<T, T2>(T param1, T2 param2);
    public static event PlayerAction<Int32, AbstractCharacter> OnWeaponContact;

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        // ── Enemy ────────────────────────────────────────────────────────────
        var enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            OnWeaponContact?.Invoke(swordOffense, enemy);
            var blinker = collider.GetComponent<ColorBlinker>();
            if (blinker != null) blinker.enabled = true;
            return;
        }

        // ── Hostile NPC ──────────────────────────────────────────────────────
        // NPCs that have been flagged as hostile receive weapon hits exactly
        // like enemies.  Passive NPCs are ignored by the sword.
        var npc = collider.GetComponent<NPC>();
        if (npc != null && npc.isHostile)
        {
            OnWeaponContact?.Invoke(swordOffense, npc);
            var blinker = collider.GetComponent<ColorBlinker>();
            if (blinker != null) blinker.enabled = true;
        }
    }
}
