using UnityEngine;

// Simulates VR-style building using a forward ray and visible laser
public class VRBuilder : MonoBehaviour
{
    public Transform rayOrigin;
    public float rayDistance = 50f;

    public LayerMask groundLayer;
    public Transform cellHighlight;

    public BuildableOption[] buildOptions;
    int currentBuildIndex = 0;

    [Header("Laser Visuals")]
    public LineRenderer laserLine;
    public Transform hitMarker;

    void Start()
    {
        UpdateBuildModeUI();
    }

    void Update()
    {
        HandleInput();
        UpdateRayVisuals();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetBuildMode(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetBuildMode(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetBuildMode(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetBuildMode(3);
    }

    void SetBuildMode(int index)
    {
        if (buildOptions == null || index >= buildOptions.Length)
            return;

        currentBuildIndex = index;
        Debug.Log("VR Build Mode: " + buildOptions[index].name);
        UpdateBuildModeUI();
    }

    void UpdateBuildModeUI()
    {
        if (UIManager.Instance == null || buildOptions == null || buildOptions.Length == 0)
            return;

        UIManager.Instance.UpdateBuildMode(buildOptions[currentBuildIndex].name);
    }

    void UpdateRayVisuals()
    {
        if (rayOrigin == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        Vector3 rayEnd = rayOrigin.position + rayOrigin.forward * rayDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            rayEnd = hit.point;

            if (hitMarker != null)
            {
                hitMarker.position = hit.point;
                hitMarker.gameObject.SetActive(true);
            }

            if (GridManager.Instance != null && cellHighlight != null)
            {
                if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
                {
                    Vector3 cellWorldPos = GridManager.Instance.GetWorldPosition(x, y);
                    cellHighlight.position = cellWorldPos + new Vector3(0f, 0.05f, 0f);
                    cellHighlight.gameObject.SetActive(true);
                }
                else
                {
                    cellHighlight.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);

            if (cellHighlight != null)
                cellHighlight.gameObject.SetActive(false);
        }

        if (laserLine != null)
        {
            laserLine.positionCount = 2;
            laserLine.SetPosition(0, rayOrigin.position);
            laserLine.SetPosition(1, rayEnd);
        }
    }

    void TryPlace()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        if (buildOptions == null || buildOptions.Length == 0)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                BuildableOption option = buildOptions[currentBuildIndex];

                if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(option.cost))
                {
                    Debug.Log("Not enough money for " + option.name);
                    return;
                }

                bool placedSuccessfully = false;

                switch (option.type)
                {
                    case BuildType.Wall:
                        placedSuccessfully = TryPlaceWall(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Turret:
                        placedSuccessfully = TryPlaceTurret(x, y, option.prefab, option.cost);
                        break;

                    case BuildType.Trap:
                        placedSuccessfully = TryPlaceTrap(x, y, option.prefab, option.cost);
                        break;
                }

                if (placedSuccessfully && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendMoney(option.cost);
                }
            }
        }
    }

    bool TryPlaceWall(int x, int y, GameObject prefab, int cost)
    {
        if (prefab == null)
            return false;

        if (IsSpecialCell(x, y))
            return false;

        if (GridManager.Instance.HasWall(x, y))
            return false;

        if (WouldBlockPath(x, y))
            return false;

        bool placed = GridManager.Instance.PlaceWall(x, y, prefab, cost);

        if (placed && pathTester != null)
        {
            pathTester.TestPath();
        }

        return placed;
    }

    bool TryPlaceTurret(int x, int y, GameObject prefab, int cost)
    {
        if (prefab == null)
            return false;

        if (!GridManager.Instance.HasWall(x, y))
            return false;

        if (GridManager.Instance.HasTurret(x, y))
            return false;

        return GridManager.Instance.PlaceTurret(x, y, prefab, cost);
    }

    bool TryPlaceTrap(int x, int y, GameObject prefab, int cost)
    {
        if (prefab == null)
            return false;

        if (GridManager.Instance.HasWall(x, y))
            return false;

        if (GridManager.Instance.HasTrap(x, y))
            return false;

        if (IsSpecialCell(x, y))
            return false;

        return GridManager.Instance.PlaceTrap(x, y, prefab, cost);
    }

    bool IsSpecialCell(int x, int y)
    {
        return false;
    }

    bool WouldBlockPath(int x, int y)
    {
        if (GridManager.Instance == null || Pathfinder.Instance == null)
            return true;

        int startX, startY, goalX, goalY;

        GameObject startMarker = GameObject.Find("StartMarker");
        GameObject endMarker = GameObject.Find("EndMarker");

        if (startMarker == null || endMarker == null)
            return true;

        if (!GridManager.Instance.GetXY(startMarker.transform.position, out startX, out startY))
            return true;

        if (!GridManager.Instance.GetXY(endMarker.transform.position, out goalX, out goalY))
            return true;

        GridManager.Instance.SetCellBlocked(x, y, true);

        var testPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        GridManager.Instance.SetCellBlocked(x, y, false);

        return testPath == null;
    }

    public PathTester pathTester;
}