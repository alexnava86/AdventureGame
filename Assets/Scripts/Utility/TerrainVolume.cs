// =============================================================================
// TerrainVolume.cs   —   Assets/Scripts/Utilities/
//
// Attach to any GameObject whose Collider2D represents an AREA the player
// can move through (water body, quicksand pit, lava zone, etc.).
//
// Setup checklist:
//   ✓  Collider2D on same GameObject — set IsTrigger = true
//   ✓  GameObject assigned to the correct physics layer (WATER, QUICKSAND …)
//   ✓  TerrainModifiers ScriptableObject assigned below
//
// Tiled integration:
//   In MapManager, after spawning a volume object from Tiled:
//     var tv = volumeObj.AddComponent<TerrainVolume>();
//     tv.modifiers = Resources.Load<TerrainModifiers>("Terrain/" + tiledTerrainProp);
//
// Physics 2D Layer matrix tip (Project Settings → Physics 2D → Layer Collision Matrix):
//   • WATER vs GROUND        : OFF (water does not collide with ground)
//   • WATER vs Player        : ON  (trigger fires when player enters)
//   • QUICKSAND vs GROUND    : OFF
//   • QUICKSAND vs Player    : ON
//   All volume layers should only trigger against the Player layer, nothing else.
// =============================================================================

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TerrainVolume : MonoBehaviour
{
    [Tooltip("Physics and hazard modifiers applied while the player is inside this volume.")]
    public TerrainModifiers modifiers;

    private void Awake()
    {
        // Enforce trigger mode — a volume must never be a solid collider.
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[TerrainVolume] Collider2D on '{name}' was not a trigger. " +
                              "Automatically set to IsTrigger = true.", this);
        }
    }
}
