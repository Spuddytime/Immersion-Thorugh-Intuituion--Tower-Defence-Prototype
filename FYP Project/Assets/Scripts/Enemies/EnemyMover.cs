using System.Collections.Generic;
using UnityEngine;

// Moves an enemy along a path and recalculates if the maze changes
public class EnemyMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float turnSpeed = 8f;
    public Transform goalMarker;

    private List<GridNode> path;
    private int currentPathIndex = 0;

    private void OnEnable()
    {
        PathTester.OnPathUpdated += RecalculatePath;
    }

    private void OnDisable()
    {
        PathTester.OnPathUpdated -= RecalculatePath;
    }

    public void SetPath(List<GridNode> newPath)
    {
        path = newPath;
        currentPathIndex = 0;

        if (path != null && path.Count > 0)
        {
            transform.position = path[0].worldPosition + Vector3.up * 0.5f;
        }
    }

    void Update()
    {
        if (path == null || path.Count == 0)
            return;

        if (currentPathIndex >= path.Count)
            return;

        Vector3 targetPosition = path[currentPathIndex].worldPosition + Vector3.up * 0.5f;
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0f;

        // Smoothly rotate toward movement direction
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Move toward current path node
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // When close enough, move to next node
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex >= path.Count)
            {
                Debug.Log("Enemy reached the goal.");
                Destroy(gameObject);
            }
        }
    }

    void RecalculatePath()
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null || goalMarker == null)
            return;

        if (!GridManager.Instance.GetXY(transform.position, out int currentX, out int currentY))
            return;

        if (!GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return;

        List<GridNode> newPath = Pathfinder.Instance.FindPath(currentX, currentY, goalX, goalY);

        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            currentPathIndex = 0;
        }
    }
}