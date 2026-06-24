// =============================================================================
// HUDManager.cs   —   Assets/Scripts/UI/
//
// Controls which HUD elements are visible based on the player's chosen HudMode
// in UISettings. Implements IUISettingsConsumer so it updates the instant the
// player changes the setting.
//
// HUD MODES:
//   Continuous       — full HUD always visible (default)
//   Discrete         — HUD hidden, flashed briefly on damage/attack, then fades
//   Minimal          — only the "minimal" subset of elements, always visible
//   MinimalDiscrete  — minimal subset, only flashed on damage/attack
//   Hidden           — nothing shown
//
// SETUP:
//   • Put this on your HUD Canvas (or an empty manager object).
//   • Drag your HP / MP / Endurance meter root objects into the lists below.
//   • Mark which elements count as "minimal" by putting them in minimalElements.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour, IUISettingsConsumer
{
    [Header("All HUD Elements")]
    [Tooltip("Every HUD element root (HP bar, MP bar, Endurance bar, etc.).")]
    public List<GameObject> allElements = new List<GameObject>();

    [Header("Minimal Subset")]
    [Tooltip("The elements that remain visible in Minimal / MinimalDiscrete modes. " +
             "Should be a subset of All Elements (e.g. just the HP bar).")]
    public List<GameObject> minimalElements = new List<GameObject>();

    [Header("Discrete Behaviour")]
    [Tooltip("Seconds the HUD stays visible after a damage/attack event in " +
             "Discrete / MinimalDiscrete modes.")]
    public float discreteShowDuration = 2f;

    // ── State ─────────────────────────────────────────────────────────────────
    private HudMode _mode = HudMode.Continuous;
    private float   _discreteTimer;

    // =========================================================================
    // Lifecycle
    // =========================================================================

    private void OnEnable()
    {
        // Apply whatever mode is currently saved as soon as the HUD loads.
        if (UISettings.Instance != null)
            OnSettingsChanged(UISettings.Instance);

        // Subscribe to the events that should "wake" the HUD in discrete modes.
        // Player damage already fires through this event in your project:
        Player.OnPlayerDamage += HandlePlayerEvent;

        // NOTE: there is no player-attack event yet. When you add one (e.g. an
        // OnPlayerAttack event on the player or weapon), subscribe to it here
        // and call FlashHUD() so attacking also reveals the HUD in discrete modes.
        // Example:
        //   PlayerSword.OnPlayerAttack += HandlePlayerEvent;
    }

    private void OnDisable()
    {
        Player.OnPlayerDamage -= HandlePlayerEvent;
        // PlayerSword.OnPlayerAttack -= HandlePlayerEvent;   // when it exists
    }

    private void Update()
    {
        // In discrete modes, count down and re-hide once the flash window passes.
        if (_discreteTimer > 0f)
        {
            _discreteTimer -= Time.deltaTime;
            if (_discreteTimer <= 0f)
                ApplyVisibility(false);   // window over → hide again
        }
    }

    // =========================================================================
    // IUISettingsConsumer
    // =========================================================================

    public void OnSettingsChanged(UISettings s)
    {
        if (s == null) return;
        _mode = s.hudMode;

        switch (_mode)
        {
            case HudMode.Continuous:
                ApplyVisibility(true);
                break;

            case HudMode.Minimal:
                ApplyVisibility(true);   // ApplyVisibility respects minimal subset below
                break;

            case HudMode.Discrete:
            case HudMode.MinimalDiscrete:
                ApplyVisibility(false);  // hidden until an event flashes it
                break;

            case HudMode.Hidden:
                ApplyVisibility(false);
                break;
        }
    }

    // =========================================================================
    // Visibility
    // =========================================================================

    /// <summary>
    /// Shows or hides HUD elements according to the active mode.
    /// 'show' is the request; the mode decides which elements it actually affects.
    /// </summary>
    private void ApplyVisibility(bool show)
    {
        bool minimalOnly = (_mode == HudMode.Minimal || _mode == HudMode.MinimalDiscrete);

        foreach (var el in allElements)
        {
            if (el == null) continue;

            if (_mode == HudMode.Hidden)
            {
                el.SetActive(false);
            }
            else if (minimalOnly)
            {
                // Only the minimal subset can ever be visible in these modes.
                bool isMinimal = minimalElements.Contains(el);
                el.SetActive(isMinimal && show);
            }
            else
            {
                el.SetActive(show);
            }
        }
    }

    /// <summary>Reveal the HUD briefly — used by discrete modes on damage/attack.</summary>
    public void FlashHUD()
    {
        if (_mode == HudMode.Discrete || _mode == HudMode.MinimalDiscrete)
        {
            ApplyVisibility(true);
            _discreteTimer = discreteShowDuration;
        }
    }

    // Player.OnPlayerDamage passes an int (hp percent); we ignore the value here.
    private void HandlePlayerEvent(int _) => FlashHUD();
}
