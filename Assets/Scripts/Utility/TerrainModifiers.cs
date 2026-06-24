// =============================================================================
// TerrainModifiers.cs   —   Assets/Scripts/Utilities/
//
// Create via: Assets ▸ Create ▸ Game ▸ Terrain Modifiers
//
// Used by BOTH surface terrain (TerrainType — collision-based) and
// volume terrain (TerrainVolume — trigger-based).  Fields that only
// make sense for volumes are grouped and documented accordingly.
// =============================================================================

using UnityEngine;

[CreateAssetMenu(fileName = "TerrainModifiers", menuName = "Game/Terrain Modifiers")]
public class TerrainModifiers : ScriptableObject
{
    // ── Movement ──────────────────────────────────────────────────────────────

    [Header("Movement")]
    [Tooltip("Speed multiplier. 0.7 = mud/water (slower). 1.0 = normal. 1.2 = downhill.")]
    public float speedMultiplier = 1f;

    [Tooltip("Friction multiplier on deceleration. 1.0 = normal. 0.05 = very icy.")]
    public float frictionMultiplier = 1f;

    [Tooltip("Override the character controller's groundAcceleration on this surface.")]
    public bool  overrideAcceleration = false;
    [Tooltip("Ground acceleration used when Override Acceleration is true (px/s²).")]
    public float groundAcceleration   = 9999f;

    // ── Jump ──────────────────────────────────────────────────────────────────

    [Header("Jump")]
    [Tooltip("Jump height multiplier. 0.8 = sand/water. 2.2 = spring pad. " +
             "Set preventJump instead of 0 if you want no jump at all.")]
    public float jumpMultiplier = 1f;

    [Tooltip("Block jumping entirely (e.g. deep quicksand, fully submerged).")]
    public bool  preventJump = false;

    [Tooltip("Allow unlimited jumps while inside this volume (e.g. swimming in water). " +
             "Each press still fires a jump, so the player can swim upward by tapping jump repeatedly. " +
             "preventJump takes priority — both cannot be true at once.")]
    public bool  infiniteJump = false;

    // ── Gravity / Fall ────────────────────────────────────────────────────────

    [Header("Gravity / Fall")]
    [Tooltip("Multiplier on the fall-gravity multiplier. 0.3 = floaty descent (shallow water).")]
    public float fallGravityMultiplier = 1f;

    [Tooltip("Scales the ENTIRE gravity arc (ascent + apex + descent). " +
             "Use this for volumes like water where everything should feel floaty, not just falling.")]
    public float volumeGravityScale = 1f;

    // ── Volume-only: continuous forces & hazards ──────────────────────────────

    [Header("Volume Effects  (TerrainVolume / trigger-based only)")]
    [Tooltip("Continuous downward acceleration applied every FixedUpdate (px/s²). " +
             "0 = none.  Use ~250 for quicksand that slowly sinks the player.")]
    public float continuousDownforce = 0f;

    [Tooltip("This volume will kill the player after Lethal Delay seconds of continuous exposure.")]
    public bool  isLethal   = false;

    [Tooltip("Seconds before death when Is Lethal is true. Timer resets on exit.")]
    public float lethalDelay = 4f;

    // ── Surface name (shared by audio / VFX) ─────────────────────────────────

    [Header("Identity")]
    [Tooltip("Name used by audio and VFX systems to play the correct footstep / splash effect.")]
    public string surfaceName = "Default";

    // =========================================================================
    // Editor preset factories
    // =========================================================================
#if UNITY_EDITOR
    public static TerrainModifiers CreateIce()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "Ice";
        t.surfaceName             = "Ice";
        t.frictionMultiplier      = 0.04f;
        t.overrideAcceleration    = true;
        t.groundAcceleration      = 300f;
        return t;
    }

    public static TerrainModifiers CreateSand()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "Sand";
        t.surfaceName             = "Sand";
        t.speedMultiplier         = 0.65f;
        t.frictionMultiplier      = 1.8f;
        t.jumpMultiplier          = 0.8f;
        return t;
    }

    public static TerrainModifiers CreateShallowWater()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "ShallowWater";
        t.surfaceName             = "Water";
        t.speedMultiplier         = 0.55f;
        t.jumpMultiplier          = 0.75f;
        t.fallGravityMultiplier   = 0.35f;
        t.volumeGravityScale      = 0.4f;   // whole arc is floaty
        return t;
    }

    public static TerrainModifiers CreateDeepWater()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "DeepWater";
        t.surfaceName             = "Water";
        t.speedMultiplier         = 0.35f;
        t.jumpMultiplier          = 0.5f;
        t.fallGravityMultiplier   = 0.2f;
        t.volumeGravityScale      = 0.25f;
        t.isLethal                = true;   // drowning — connect to O2 meter later
        t.lethalDelay             = 8f;
        return t;
    }

    public static TerrainModifiers CreateQuicksand()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "Quicksand";
        t.surfaceName             = "Quicksand";
        t.speedMultiplier         = 0.3f;
        t.jumpMultiplier          = 0.45f;  // very hard to jump out
        t.continuousDownforce     = 250f;   // constantly sinks the player
        t.isLethal                = true;
        t.lethalDelay             = 5f;
        return t;
    }

    public static TerrainModifiers CreateSpring()
    {
        var t = CreateInstance<TerrainModifiers>();
        t.name                    = "Spring";
        t.surfaceName             = "Spring";
        t.jumpMultiplier          = 2.2f;
        return t;
    }
#endif
}
