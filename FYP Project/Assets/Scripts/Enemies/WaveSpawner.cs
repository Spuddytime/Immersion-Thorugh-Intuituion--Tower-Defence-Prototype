using System.Collections;
using UnityEngine;

// Controls enemy waves by repeatedly calling EnemySpawner
public class WaveSpawner : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    public int enemiesPerWave = 5;
    public float timeBetweenSpawns = 1f;

    private bool isSpawning = false;
    private int currentWave = 0;

    void Update()
    {
        // Press W to start the next wave
        if (Input.GetKeyDown(KeyCode.W) && !isSpawning)
        {
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        if (enemySpawner == null)
            yield break;

        currentWave++;
        isSpawning = true;

        Debug.Log("Wave " + currentWave + " started.");

        for (int i = 0; i < enemiesPerWave; i++)
        {
            enemySpawner.SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("Wave " + currentWave + " finished spawning.");

        isSpawning = false;
    }
}