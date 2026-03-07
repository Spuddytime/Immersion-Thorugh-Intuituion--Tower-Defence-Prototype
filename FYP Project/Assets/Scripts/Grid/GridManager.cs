using System.Collections.Generic;
using UnityEngine;

// Handles creation and management of the grid used for building and pathfinding
public class GridManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public Vector3 originPosition = Vector3.zero;

    private GridNode[,] grid;

    public static GridManager Instance;

    private void Awake()
    {
        Instance = this;
        CreateGrid();
    }

    // Creates the full grid of nodes
    void CreateGrid()
    {
        grid = new GridNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);
                grid[x, y] = new GridNode(x, y, worldPos);
            }
        }
    }

    // Converts grid coordinates into a world position
    public Vector3 GetWorldPosition(int x, int y)
    {
        return originPosition + new Vector3(
            x * cellSize + cellSize * 0.5f,
            0f,
            y * cellSize + cellSize * 0.5f
        );
    }

    // Converts a world position into grid coordinates
    public bool GetXY(Vector3 worldPosition, out int x, out int y)
    {
        Vector3 local = worldPosition - originPosition;

        x = Mathf.FloorToInt(local.x / cellSize);
        y = Mathf.FloorToInt(local.z / cellSize);

        return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
    }

    // Returns the node at the given grid coordinates
    public GridNode GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            return null;

        return grid[x, y];
    }

    // Returns neighbouring cells (no diagonals)
    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        // Up
        if (node.y + 1 < gridHeight)
            neighbours.Add(grid[node.x, node.y + 1]);

        // Right
        if (node.x + 1 < gridWidth)
            neighbours.Add(grid[node.x + 1, node.y]);

        // Down
        if (node.y - 1 >= 0)
            neighbours.Add(grid[node.x, node.y - 1]);

        // Left
        if (node.x - 1 >= 0)
            neighbours.Add(grid[node.x - 1, node.y]);

        return neighbours;
    }

    // Returns true if the given cell is blocked
    public bool IsCellBlocked(int x, int y)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return true;

        return node.isBlocked;
    }

    // Temporarily sets whether a cell is blocked
    public void SetCellBlocked(int x, int y, bool blocked)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return;

        node.isBlocked = blocked;
    }

    // Places an object (like a wall) in a grid cell
    public bool PlaceObject(int x, int y, GameObject prefab)
    {
        GridNode node = GetNode(x, y);

        if (node == null || node.isBlocked)
            return false;

        GameObject obj = Instantiate(prefab, node.worldPosition, Quaternion.identity);

        node.isBlocked = true;
        node.placedObject = obj;

        return true;
    }

    // Removes any object from the specified cell
    public void ClearCell(int x, int y)
    {
        GridNode node = GetNode(x, y);

        if (node == null)
            return;

        if (node.placedObject != null)
        {
            Destroy(node.placedObject);
        }

        node.isBlocked = false;
        node.placedObject = null;
    }

    // Draws the grid in the Scene view for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = originPosition + new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    0f,
                    y * cellSize + cellSize * 0.5f
                );

                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.05f, cellSize));
            }
        }
    }
}