using System.Collections.Generic;
using UnityEngine;

// Moves an enemy along the current path from start to goal
public class EnemyMover : MonoBehaviour
{
    public float moveSpeed = 2f;

    private List<GridNode> path;
    private int currentPathIndex = 0;

    public void SetPath(List<GridNode> newPath)
    {
        path = newPath;
        currentPathIndex = 0;

        // Place enemy at the first node in the path
        if (path != null && path.Count > 0)
        {
            transform.position = path[0].worldPosition + Vector3.up * 0.5f;
        }
    }

    void Update()
    {
        if (path == null || path.Count == 0)
            return;

        // If we've reached the end of the path, stop moving
        if (currentPathIndex >= path.Count)
            return;

        Vector3 targetPosition = path[currentPathIndex].worldPosition + Vector3.up * 0.5f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // If close enough to the current node, move to the next one
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentPathIndex++;

            // If enemy has reached the goal, destroy it
            if (currentPathIndex >= path.Count)
            {
                Debug.Log("Enemy reached the goal.");
                Destroy(gameObject);
            }
        }
    }
}