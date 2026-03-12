using UnityEngine;

// Represents a single cell in the grid
public class GridNode
{
    public int x;
    public int y;

    public bool isBlocked;

    public Vector3 worldPosition;

    public GameObject wallObject;
    public GameObject turretObject;
    public GameObject trapObject;

    public GridNode cameFromNode;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;

        isBlocked = false;
        wallObject = null;
        turretObject = null;
        trapObject = null;
        cameFromNode = null;
    }
}