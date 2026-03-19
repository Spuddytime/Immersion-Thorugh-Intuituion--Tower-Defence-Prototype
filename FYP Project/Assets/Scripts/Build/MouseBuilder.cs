using UnityEngine;

// Handles player input for placing walls, mounted turrets, and traps using the mouse
public class MouseBuilder : MonoBehaviour
{
    public Camera mainCamera;
    public BuildableOption[] buildOptions;
    public Transform cellHighlight;
    public LayerMask groundLayer;

    public Transform startMarker;
    public Transform goalMarker;

    public PathTester pathTester;

    private BuildMode currentBuildMode = BuildMode.Wall;

    private enum BuildMode
    {
        Wall,
        Turret,
        Trap
    }

    void Start()
    {
        UpdateBuildModeUI();
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
            UpdateBuildModeUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBuildMode = BuildMode.Turret;
            Debug.Log("Build Mode: Turret");
            UpdateBuildModeUI();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentBuildMode = BuildMode.Trap;
            Debug.Log("Build Mode: Trap");
            UpdateBuildModeUI();
        }
    }

    void UpdateBuildModeUI()
    {
        if (UIManager.Instance == null)
            return;

        switch (currentBuildMode)
        {
            case BuildMode.Wall:
                UIManager.Instance.UpdateBuildMode("Wall");
                break;

            case BuildMode.Turret:
                UIManager.Instance.UpdateBuildMode("Turret");
                break;

            case BuildMode.Trap:
                UIManager.Instance.UpdateBuildMode("Trap");
                break;
        }
    }

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

                    case BuildMode.Trap:
                        TryPlaceTrap(x, y);
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

    void TryPlaceTrap(int x, int y)
{
    if (trapPrefab == null)
        return;

    // Traps go on walkable ground, not on walls
    if (GridManager.Instance.HasWall(x, y))
         {   
        Debug.Log("Traps must be placed on open ground, not on walls.");
        return;
        }

    if (GridManager.Instance.HasTrap(x, y))
        {
        Debug.Log("This tile already has a trap.");
        return;
        }

    if (IsSpecialCell(x, y))
        {
        Debug.Log("Cannot place trap on start or goal cell.");
        return;
        }

    GridManager.Instance.PlaceTrap(x, y, trapPrefab);
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