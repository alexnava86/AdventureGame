using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemyPrefab; // The enemy prefab to spawn.
    //public float respawnDelay = 0.5f; // Delay before respawning in seconds.
    //private bool isRespawning = false;

    void OnEnable()
    {
        Enemy.OnDeath += InstantiateClones;
    }

    void OnDisable()
    {
        Enemy.OnDeath -= InstantiateClones;
    }

    private void Start()
    {
        enemyPrefab = Resources.Load("Prefabs/Enemies/Scuttler") as GameObject;
    }

    private void InstantiateClones()
    {
        //isRespawning = true;
        Instantiate(enemyPrefab, new Vector2(this.transform.position.x - 32f, this.transform.position.y), Quaternion.identity);
        Instantiate(enemyPrefab, new Vector2(this.transform.position.x + 32f, this.transform.position.y), Quaternion.identity);
        Destroy(this.gameObject);
    }

    /*
    private IEnumerator RespawnEnemy()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Instantiate the enemy at the same position.
        Instantiate(enemyPrefab, this.transform.position, Quaternion.identity);

        // Destroy the current (destroyed) enemy.
    }
    */
}
