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
        if (Input.GetKeyDown(KeyCode.E) && !isSpawning)
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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(currentWave);
        }

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