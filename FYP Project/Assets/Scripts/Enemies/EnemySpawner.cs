using System.Collections.Generic;
using UnityEngine;

// Simple test spawner for creating one enemy on key press
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public PathTester pathTester;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null || pathTester == null)
            return;

        List<GridNode> path = pathTester.GetCurrentPath();

        if (path == null || path.Count == 0)
        {
            Debug.Log("Cannot spawn enemy - no valid path.");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab);
        EnemyMover mover = enemy.GetComponent<EnemyMover>();

        if (mover != null)
        {
            mover.goalMarker = pathTester.goalMarker;
            mover.SetPath(path);
        }
    }
}