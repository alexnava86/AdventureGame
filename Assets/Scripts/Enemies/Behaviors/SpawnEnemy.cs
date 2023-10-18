using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemyPrefab; // The enemy prefab to spawn.
    public float respawnDelay = 0.5f; // Delay before respawning in seconds.

    private bool isRespawning = false;

    // Handle enemy behavior (movement, attack, etc.) here.

    private void OnDestroy()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            StartCoroutine(RespawnEnemy());
        }
    }

    private IEnumerator RespawnEnemy()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Instantiate the enemy at the same position.
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        // Destroy the current (destroyed) enemy.
        Destroy(gameObject);
    }
}
