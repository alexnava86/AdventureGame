// =============================================================================
// TimeManager.cs   —   Assets/Scripts/Utility/
//
// Drives the day/night look by tinting the TimeFilterScreen Image (and its
// material) based on the current hour of the in-game day. Each hour can have
// its own color + opacity, defined as a gradient of keyframes so the look
// blends smoothly between the times you set.
//
// Attach to the TimeManager prefab (on the Main Camera). Assign the
// TimeFilterScreen's Image to `filterImage`.
//
// TESTING: use the "Current Hour" slider in the Inspector (0–24) to scrub
// through the day and watch the filter update live, even when not playing.
// A companion editor (TimeManagerEditor) adds quick preset buttons.
//
// LATER: when the overworld time system exists, advance `currentHour` over
// real time while the player is in an overworld scene, and this filter follows.
// =============================================================================

using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]   // updates the filter in the editor too, not just at play time
public class TimeManager : MonoBehaviour
{
    [Header("Filter Target")]
    [Tooltip("The Image on the TimeFilterScreen child that tints the view.")]
    public Image filterImage;
    [Tooltip("Optional: also drive a material color property (for shader effects). " +
             "Leave blank to only tint the Image.")]
    public string materialColorProperty = "_Color";

    [Header("Time of Day")]
    [Tooltip("Current hour, 0–24 (e.g. 13.5 = 1:30 PM). Scrub this to preview times.")]
    [Range(0f, 24f)]
    public float currentHour = 22f;   // matches the scene's current ~10pm test value

    [Header("Day Color Gradient")]
    [Tooltip("The filter color across the day. Use the time keys (0–1 = midnight→midnight).")]
    public Gradient dayColor = DefaultDayColor();
    [Tooltip("The filter opacity across the day (0 = clear/bright, 1 = fully tinted/dark). " +
             "Keys are 0–1 across 24 hours.")]
    public AnimationCurve dayOpacity = DefaultDayOpacity();

    // =========================================================================
    // Lifecycle
    // =========================================================================

    private void OnEnable()  => ApplyFilter();
    private void Update()
    {
        // In the editor (ExecuteAlways) this lets the slider update live.
        // At runtime it's cheap; later the overworld system will move currentHour.
        ApplyFilter();
    }

    // When a value changes in the Inspector, refresh immediately.
    private void OnValidate() => ApplyFilter();

    // =========================================================================
    // Core
    // =========================================================================

    /// <summary>Sets the filter color + opacity for the current hour.</summary>
    public void ApplyFilter()
    {
        if (filterImage == null) return;

        float t = Mathf.Repeat(currentHour, 24f) / 24f;   // 0..1 across the day

        Color c = dayColor.Evaluate(t);
        c.a     = Mathf.Clamp01(dayOpacity.Evaluate(t));

        filterImage.color = c;

        // Optionally push the color into the material too (for custom shaders)
        if (!string.IsNullOrEmpty(materialColorProperty) &&
            filterImage.material != null &&
            filterImage.material.HasProperty(materialColorProperty))
        {
            filterImage.material.SetColor(materialColorProperty, c);
        }
    }

    /// <summary>Set the time of day directly (0–24). Use from gameplay later.</summary>
    public void SetHour(float hour)
    {
        currentHour = Mathf.Repeat(hour, 24f);
        ApplyFilter();
    }

    // =========================================================================
    // Defaults — a reasonable starting day cycle you can tweak in the Inspector
    // Times are 0..1 where 0 = midnight, 0.5 = noon, 1 = next midnight.
    // Colors are tints; opacity curve controls how strong the tint is.
    // =========================================================================

    private static Gradient DefaultDayColor()
    {
        var g = new Gradient();
        g.colorKeys = new[]
        {
            new GradientColorKey(new Color(0.03f, 0.00f, 0.13f), 0.00f), // midnight — deep blue
            new GradientColorKey(new Color(0.10f, 0.05f, 0.20f), 0.21f), // ~5am — pre-dawn
            new GradientColorKey(new Color(1.00f, 0.65f, 0.40f), 0.29f), // ~7am — sunrise warm
            new GradientColorKey(new Color(1.00f, 1.00f, 1.00f), 0.42f), // ~10am — clear daylight
            new GradientColorKey(new Color(1.00f, 1.00f, 1.00f), 0.58f), // ~2pm — clear daylight
            new GradientColorKey(new Color(1.00f, 0.55f, 0.30f), 0.75f), // ~6pm — sunset warm
            new GradientColorKey(new Color(0.20f, 0.10f, 0.30f), 0.83f), // ~8pm — dusk
            new GradientColorKey(new Color(0.03f, 0.00f, 0.13f), 1.00f), // midnight — deep blue
        };
        g.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
        return g;
    }

    private static AnimationCurve DefaultDayOpacity()
    {
        // 0 = no tint (full daylight), up toward ~0.7 at deep night.
        return new AnimationCurve(
            new Keyframe(0.00f, 0.70f),  // midnight — darkest
            new Keyframe(0.08f, 0.72f),  // ~2am — slightly darker peak
            new Keyframe(0.25f, 0.45f),  // ~6am — dawn lifting
            new Keyframe(0.33f, 0.10f),  // ~8am — almost clear
            new Keyframe(0.50f, 0.00f),  // noon — fully clear
            new Keyframe(0.67f, 0.10f),  // ~4pm — almost clear
            new Keyframe(0.79f, 0.45f),  // ~7pm — dusk falling
            new Keyframe(0.92f, 0.70f),  // ~10pm — dark
            new Keyframe(1.00f, 0.70f)   // midnight — darkest
        );
    }
}
