// =============================================================================
// SpawnEnemy.cs   —   Assets/Scripts/Enemies/Behaviors/
//
// When this enemy dies, it spawns child enemies that arc outward from the
// death position (like a Metroid splitting). Each spawn point is checked
// against solid ground first, so children never spawn embedded in walls or
// floors where the player can't reach them.
// =============================================================================

using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [Header("Spawn")]
    [Tooltip("The enemy prefab to spawn on death.")]
    public GameObject enemyPrefab;
    [Tooltip("How many children to spawn.")]
    public int spawnCount = 2;

    [Header("Arc Launch")]
    [Tooltip("Horizontal launch speed of spawned children (px/s).")]
    public float launchSpeedX = 90f;
    [Tooltip("Upward launch speed of spawned children (px/s).")]
    public float launchSpeedY = 200f;

    [Header("Ground Avoidance")]
    [Tooltip("Layer(s) considered solid — spawns are nudged out of these.")]
    public LayerMask groundLayer;
    [Tooltip("Radius used to test whether a spawn point overlaps solid ground (px).")]
    public float overlapCheckRadius = 6f;
    [Tooltip("Vertical distance to lift a blocked spawn point looking for free space (px).")]
    public float clearanceStep = 8f;
    [Tooltip("Max attempts to find clear space before giving up on that spawn.")]
    public int maxClearanceTries = 6;

    private void OnEnable()  { Enemy.OnDeath += InstantiateClones; }
    private void OnDisable() { Enemy.OnDeath -= InstantiateClones; }

    private void InstantiateClones(AbstractCharacter sender)
    {
        if (sender != this.GetComponent<AbstractCharacter>()) return;
        if (enemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // Alternate left/right; spread extras if spawnCount > 2
            float side = (i % 2 == 0) ? -1f : 1f;
            float tier = i / 2;   // 0,0,1,1,2,2...

            Vector2 basePos = (Vector2)transform.position
                            + new Vector2(side * (16f + tier * 8f), 16f);

            Vector2 spawnPos = FindClearSpawn(basePos);

            var clone = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Launch in an arc away from the death point
            var rb = clone.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = new Vector2(side * launchSpeedX, launchSpeedY);
        }
    }

    /// <summary>
    /// Returns a spawn position that is not embedded in solid ground.
    /// If the requested point overlaps ground, it lifts the point upward in
    /// steps until clear, so spawned enemies are always reachable.
    /// </summary>
    private Vector2 FindClearSpawn(Vector2 desired)
    {
        Vector2 test = desired;
        for (int t = 0; t < maxClearanceTries; t++)
        {
            bool blocked = Physics2D.OverlapCircle(test, overlapCheckRadius, groundLayer);
            if (!blocked) return test;
            test += Vector2.up * clearanceStep;   // lift out of the ground
        }
        // Could not find clear space — return the highest point we tried
        return test;
    }
}
