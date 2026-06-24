// =============================================================================
// TerrainType.cs   —   Assets/Scripts/Utilities/
//
// Attach this to any ground collider (tilemap, platform, floor object) and
// assign a TerrainModifiers ScriptableObject.  CharacterController2D reads it
// automatically via OnCollisionEnter2D — no other setup needed.
//
// Tiled integration:
//   In your MapManager, after spawning a ground prefab, call:
//
//     var tt = groundObj.AddComponent<TerrainType>();
//     tt.modifiers = Resources.Load<TerrainModifiers>("Terrain/" + tiledTerrainProperty);
//
//   Provided your TerrainModifiers assets live in Resources/Terrain/ and are named
//   to match the Tiled custom property value (e.g. "Ice", "Sand", "Water").
// =============================================================================

using UnityEngine;

/// <summary>
/// Marks a ground collider as a specific terrain type.
/// Attach to the same GameObject that has the Collider2D.
/// </summary>
public class TerrainType : MonoBehaviour
{
    [Tooltip("Physics overrides applied to any CharacterController2D that stands on this surface.")]
    public TerrainModifiers modifiers;

    // Optional: surface name used by audio/particle systems to pick the right
    // footstep sound, dust effect, etc.
    [Tooltip("Surface name for audio/VFX lookups (e.g. 'Grass', 'Ice', 'Sand').")]
    public string surfaceName = "Default";
}
