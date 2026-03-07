using UnityEngine;

// Handles player input for placing walls and mounted turrets using the mouse
public class MouseBuilder : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject wallPrefab;
    public GameObject turretPrefab;
    public Transform cellHighlight;
    public LayerMask groundLayer;

    public Transform startMarker;
    public Transform goalMarker;

    public PathTester pathTester;

    private BuildMode currentBuildMode = BuildMode.Wall;

    private enum BuildMode
    {
        Wall,
        Turret
    }

    void Update()
    {
        HandleBuildModeInput();
        UpdateHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRemove();
        }
    }

    void HandleBuildModeInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBuildMode = BuildMode.Wall;
            Debug.Log("Build Mode: Wall");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBuildMode = BuildMode.Turret;
            Debug.Log("Build Mode: Turret");
        }
    }

    // Moves the highlight object to the grid cell under the mouse
    void UpdateHighlight()
    {
        if (mainCamera == null || cellHighlight == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                Vector3 cellWorldPos = GridManager.Instance.GetWorldPosition(x, y);
                cellHighlight.position = cellWorldPos + new Vector3(0f, 0.05f, 0f);
                cellHighlight.gameObject.SetActive(true);
                return;
            }
        }

        cellHighlight.gameObject.SetActive(false);
    }

    void TryPlace()
    {
        if (mainCamera == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                switch (currentBuildMode)
                {
                    case BuildMode.Wall:
                        TryPlaceWall(x, y);
                        break;

                    case BuildMode.Turret:
                        TryPlaceTurret(x, y);
                        break;
                }
            }
        }
    }

    void TryPlaceWall(int x, int y)
    {
        if (wallPrefab == null)
            return;

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot build on start or goal cell.");
            return;
        }

        if (GridManager.Instance.HasWall(x, y))
        {
            return;
        }

        if (WouldBlockPath(x, y))
        {
            Debug.Log("Cannot place wall here - it would block all paths.");
            return;
        }

        bool placed = GridManager.Instance.PlaceWall(x, y, wallPrefab);

        if (placed && pathTester != null)
        {
            pathTester.TestPath();
        }
    }

    void TryPlaceTurret(int x, int y)
    {
        if (turretPrefab == null)
            return;

        if (!GridManager.Instance.HasWall(x, y))
        {
            Debug.Log("Turrets must be placed on an existing wall.");
            return;
        }

        if (GridManager.Instance.HasTurret(x, y))
        {
            Debug.Log("This wall already has a turret.");
            return;
        }

        GridManager.Instance.PlaceTurret(x, y, turretPrefab);
    }

    bool IsSpecialCell(int x, int y)
    {
        if (startMarker != null && GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
        {
            if (x == startX && y == startY)
                return true;
        }

        if (goalMarker != null && GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
        {
            if (x == goalX && y == goalY)
                return true;
        }

        return false;
    }

    bool WouldBlockPath(int x, int y)
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null)
            return true;

        if (startMarker == null || !GridManager.Instance.GetXY(startMarker.position, out int startX, out int startY))
            return true;

        if (goalMarker == null || !GridManager.Instance.GetXY(goalMarker.position, out int goalX, out int goalY))
            return true;

        GridManager.Instance.SetCellBlocked(x, y, true);

        var testPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        GridManager.Instance.SetCellBlocked(x, y, false);

        return testPath == null;
    }

    void TryRemove()
    {
        if (mainCamera == null || GridManager.Instance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                GridManager.Instance.ClearCell(x, y);

                if (pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }
}