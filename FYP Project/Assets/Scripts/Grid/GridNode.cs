using UnityEngine;

// Represents a single cell in the grid
public class GridNode
{
    // Grid coordinates
    public int x;
    public int y;

    // Whether this cell is blocked for pathfinding
    public bool isBlocked;

    // World position of the centre of the cell
    public Vector3 worldPosition;

    // Reference to wall placed in this cell
    public GameObject wallObject;

    // Reference to turret placed on this wall
    public GameObject turretObject;

    // Used by pathfinding to reconstruct the path
    public GridNode cameFromNode;

    public GridNode(int x, int y, Vector3 worldPosition)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPosition;

        isBlocked = false;
        wallObject = null;
        turretObject = null;
        cameFromNode = null;
    }
}