using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// VR-style builder using a world-space ray and visible laser
public class VRBuilder : MonoBehaviour
{
    [Header("Ray")]
    public Transform rayOrigin;
    public float rayDistance = 50f;
    public LayerMask groundLayer;

    [Header("Build Data")]
    public BuildableOption[] buildOptions;
    public Transform cellHighlight;

    [Header("Highlight Materials")]
    public Material validHighlightMaterial;
    public Material invalidHighlightMaterial;

    [Header("Scene References")]
    public Transform startMarker;
    public Transform goalMarker;
    public PathTester pathTester;

    [Header("Laser Visuals")]
    public LineRenderer laserLine;
    public Transform hitMarker;

    [Header("Placement Feedback")]
    public bool usePlacementPop = true;
    public float popDuration = 0.15f;
    public float popStartScaleMultiplier = 0.2f;

    [Header("Upgrade Feedback")]
    public float upgradePulseMultiplier = 1.15f;
    public float upgradePulseDuration = 0.12f;

    [Header("Haptics")]
    public bool useHaptics = true;
    public float placeHapticStrength = 0.4f;
    public float placeHapticDuration = 0.08f;
    public float removeHapticStrength = 0.25f;
    public float removeHapticDuration = 0.06f;
    public float upgradeHapticStrength = 0.5f;
    public float upgradeHapticDuration = 0.1f;
    public float cycleHapticStrength = 0.2f;
    public float cycleHapticDuration = 0.04f;

    [Header("Hover Haptics")]
    public float hoverUpgradeHapticStrength = 0.2f;
    public float hoverUpgradeHapticDuration = 0.03f;

    private int currentBuildIndex = 0;

    private InputDevice rightHand;

    private bool lastTriggerState = false;
    private bool lastGripState = false;
    private bool lastPrimaryState = false;
    private bool lastSecondaryState = false;

    private Upgradeable lastHoveredUpgradeable;

    void Start()
    {
        UpdateBuildModeUI();
        TryInitializeRightHand();
    }

    void Update()
    {
        if (!rightHand.isValid)
        {
            TryInitializeRightHand();
        }

        HandleInput();
        UpdateRayVisuals();
    }

    void TryInitializeRightHand()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHand = devices[0];
            Debug.Log("Right controller connected: " + rightHand.name);
        }
    }

    void HandleInput()
    {
        if (!rightHand.isValid)
            return;

        // Trigger = Place
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
        {
            if (triggerPressed && !lastTriggerState)
            {
                TryPlace();
            }
            lastTriggerState = triggerPressed;
        }

        // Grip = Remove
        if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed))
        {
            if (gripPressed && !lastGripState)
            {
                TryRemove();
            }
            lastGripState = gripPressed;
        }

        // A = Cycle build mode
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed))
        {
            if (primaryPressed && !lastPrimaryState)
            {
                CycleBuildMode();
            }
            lastPrimaryState = primaryPressed;
        }

        // B = Upgrade
        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPressed))
        {
            if (secondaryPressed && !lastSecondaryState)
            {
                TryUpgrade();
            }
            lastSecondaryState = secondaryPressed;
        }
    }

    void CycleBuildMode()
    {
        if (buildOptions == null || buildOptions.Length == 0)
            return;

        currentBuildIndex++;

        if (currentBuildIndex >= buildOptions.Length)
            currentBuildIndex = 0;

        Debug.Log("VR Build Mode: " + buildOptions[currentBuildIndex].name);
        UpdateBuildModeUI();
        SendHaptics(cycleHapticStrength, cycleHapticDuration);
    }

    void UpdateBuildModeUI()
    {
        if (UIManager.Instance == null || buildOptions == null || buildOptions.Length == 0)
            return;

        UIManager.Instance.UpdateBuildMode(buildOptions[currentBuildIndex].name);
        UIManager.Instance.UpdateBuildCost(buildOptions[currentBuildIndex].cost);
    }

    void UpdateRayVisuals()
    {
        if (rayOrigin == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        Vector3 startPos = rayOrigin.position + rayOrigin.forward * 0.05f;
        Vector3 rayEnd = rayOrigin.position + rayOrigin.forward * rayDistance;

        bool foundValidGridCell = false;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            rayEnd = hit.point;

            if (hitMarker != null)
            {
                hitMarker.position = hit.point + Vector3.up * 0.02f;
                hitMarker.gameObject.SetActive(true);
            }

            if (GridManager.Instance != null && cellHighlight != null)
            {
                if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
                {
                    foundValidGridCell = true;

                    Vector3 cellWorldPos = GridManager.Instance.GetWorldPosition(x, y);
                    cellHighlight.position = cellWorldPos + new Vector3(0f, 0.05f, 0f);
                    cellHighlight.gameObject.SetActive(true);

                    if (buildOptions != null && buildOptions.Length > 0)
                    {
                        BuildableOption option = buildOptions[currentBuildIndex];
                        bool isValid = IsValidPlacement(x, y, option);

                        Renderer rend = cellHighlight.GetComponentInChildren<Renderer>();
                        if (rend != null)
                        {
                            if (isValid && validHighlightMaterial != null)
                            {
                                rend.material = validHighlightMaterial;
                            }
                            else if (!isValid && invalidHighlightMaterial != null)
                            {
                                rend.material = invalidHighlightMaterial;
                            }
                        }
                    }

                    UpdateUpgradeCostUI(x, y);
                    CheckUpgradeableHoverHaptics(x, y);
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

        if (!foundValidGridCell)
        {
            lastHoveredUpgradeable = null;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUpgradeCost(-1);
            }
        }

        if (laserLine != null)
        {
            laserLine.positionCount = 2;
            laserLine.SetPosition(0, startPos);
            laserLine.SetPosition(1, rayEnd);
        }
    }

    void UpdateUpgradeCostUI(int x, int y)
    {
        if (UIManager.Instance == null || GridManager.Instance == null)
            return;

        GridNode node = GridManager.Instance.GetNode(x, y);
        if (node == null)
        {
            UIManager.Instance.UpdateUpgradeCost(-1);
            return;
        }

        Upgradeable upgradeable = null;

        if (node.turretObject != null)
            upgradeable = node.turretObject.GetComponent<Upgradeable>();
        else if (node.trapObject != null)
            upgradeable = node.trapObject.GetComponent<Upgradeable>();

        if (upgradeable != null && upgradeable.CanUpgrade())
        {
            UIManager.Instance.UpdateUpgradeCost(upgradeable.GetUpgradeCost());
        }
        else
        {
            UIManager.Instance.UpdateUpgradeCost(-1);
        }
    }

    void CheckUpgradeableHoverHaptics(int x, int y)
    {
        if (GridManager.Instance == null)
            return;

        GridNode node = GridManager.Instance.GetNode(x, y);
        if (node == null)
        {
            lastHoveredUpgradeable = null;
            return;
        }

        Upgradeable currentUpgradeable = null;

        if (node.turretObject != null)
            currentUpgradeable = node.turretObject.GetComponent<Upgradeable>();
        else if (node.trapObject != null)
            currentUpgradeable = node.trapObject.GetComponent<Upgradeable>();

        if (currentUpgradeable != null && currentUpgradeable.CanUpgrade())
        {
            if (currentUpgradeable != lastHoveredUpgradeable)
            {
                SendHaptics(hoverUpgradeHapticStrength, hoverUpgradeHapticDuration);
                lastHoveredUpgradeable = currentUpgradeable;
            }
        }
        else
        {
            lastHoveredUpgradeable = null;
        }
    }

    void SendHaptics(float amplitude, float duration)
    {
        if (!useHaptics)
            return;

        if (!rightHand.isValid)
            return;

        rightHand.SendHapticImpulse(0u, amplitude, duration);
    }

    bool IsValidPlacement(int x, int y, BuildableOption option)
    {
        if (GridManager.Instance == null || option == null)
            return false;

        switch (option.type)
        {
            case BuildType.Wall:
                if (IsSpecialCell(x, y))
                    return false;

                if (GridManager.Instance.HasWall(x, y))
                    return false;

                if (WouldBlockPath(x, y))
                    return false;

                return true;

            case BuildType.Turret:
                if (!GridManager.Instance.HasWall(x, y))
                    return false;

                if (GridManager.Instance.HasTurret(x, y))
                    return false;

                return true;

            case BuildType.Trap:
                if (IsSpecialCell(x, y))
                    return false;

                if (GridManager.Instance.HasWall(x, y))
                    return false;

                if (GridManager.Instance.HasTrap(x, y))
                    return false;

                return true;
        }

        return false;
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
                GameObject placedObject = null;

                switch (option.type)
                {
                    case BuildType.Wall:
                        placedSuccessfully = TryPlaceWall(x, y, option.prefab, option.cost, out placedObject);
                        break;

                    case BuildType.Turret:
                        placedSuccessfully = TryPlaceTurret(x, y, option.prefab, option.cost, out placedObject);
                        break;

                    case BuildType.Trap:
                        placedSuccessfully = TryPlaceTrap(x, y, option.prefab, option.cost, out placedObject);
                        break;
                }

                if (placedSuccessfully && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendMoney(option.cost);

                    if (usePlacementPop && placedObject != null)
                    {
                        StartCoroutine(PlayPlacementPop(placedObject.transform));
                    }

                    SendHaptics(placeHapticStrength, placeHapticDuration);
                }
            }
        }
    }

    bool TryPlaceWall(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

        if (prefab == null)
            return false;

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot build on start or goal cell.");
            return false;
        }

        if (GridManager.Instance.HasWall(x, y))
            return false;

        if (WouldBlockPath(x, y))
        {
            Debug.Log("Cannot place wall here - it would block all paths.");
            return false;
        }

        bool placed = GridManager.Instance.PlaceWall(x, y, prefab, cost);

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.wallObject;

            if (pathTester != null)
            {
                pathTester.TestPath();
            }
        }

        return placed;
    }

    bool TryPlaceTurret(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

        if (prefab == null)
            return false;

        if (!GridManager.Instance.HasWall(x, y))
        {
            Debug.Log("Turrets must be placed on an existing wall.");
            return false;
        }

        if (GridManager.Instance.HasTurret(x, y))
        {
            Debug.Log("This wall already has a turret.");
            return false;
        }

        bool placed = GridManager.Instance.PlaceTurret(x, y, prefab, cost);

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.turretObject;
        }

        return placed;
    }

    bool TryPlaceTrap(int x, int y, GameObject prefab, int cost, out GameObject placedObject)
    {
        placedObject = null;

        if (prefab == null)
            return false;

        if (GridManager.Instance.HasWall(x, y))
        {
            Debug.Log("Traps must be placed on open ground, not on walls.");
            return false;
        }

        if (GridManager.Instance.HasTrap(x, y))
        {
            Debug.Log("This tile already has a trap.");
            return false;
        }

        if (IsSpecialCell(x, y))
        {
            Debug.Log("Cannot place trap on start or goal cell.");
            return false;
        }

        bool placed = GridManager.Instance.PlaceTrap(x, y, prefab, cost);

        if (placed)
        {
            GridNode node = GridManager.Instance.GetNode(x, y);
            if (node != null)
                placedObject = node.trapObject;
        }

        return placed;
    }

    IEnumerator PlayPlacementPop(Transform placedTransform)
    {
        if (placedTransform == null)
            yield break;

        Vector3 finalScale = placedTransform.localScale;
        Vector3 startScale = finalScale * popStartScaleMultiplier;

        placedTransform.localScale = startScale;

        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            placedTransform.localScale = Vector3.Lerp(startScale, finalScale, t);
            yield return null;
        }

        placedTransform.localScale = finalScale;
    }

    IEnumerator PlayUpgradePulse(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 originalScale = target.localScale;
        Vector3 biggerScale = originalScale * upgradePulseMultiplier;

        float timer = 0f;

        while (timer < upgradePulseDuration)
        {
            timer += Time.deltaTime;
            float t = timer / upgradePulseDuration;
            target.localScale = Vector3.Lerp(originalScale, biggerScale, t);
            yield return null;
        }

        timer = 0f;

        while (timer < upgradePulseDuration)
        {
            timer += Time.deltaTime;
            float t = timer / upgradePulseDuration;
            target.localScale = Vector3.Lerp(biggerScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    void TryRemove()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                int refund = GridManager.Instance.ClearCell(x, y);

                if (refund > 0 && EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddMoney(refund);
                    Debug.Log("Refunded: " + refund);
                    SendHaptics(removeHapticStrength, removeHapticDuration);
                }

                if (pathTester != null)
                {
                    pathTester.TestPath();
                }
            }
        }
    }

    void TryUpgrade()
    {
        if (rayOrigin == null || GridManager.Instance == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                bool upgraded = GridManager.Instance.TryUpgradeAtCell(x, y);

                if (upgraded)
                {
                    Debug.Log("Upgraded build at cell: " + x + ", " + y);
                    SendHaptics(upgradeHapticStrength, upgradeHapticDuration);

                    GridNode node = GridManager.Instance.GetNode(x, y);
                    if (node != null)
                    {
                        if (node.turretObject != null)
                        {
                            StartCoroutine(PlayUpgradePulse(node.turretObject.transform));
                        }
                        else if (node.trapObject != null)
                        {
                            StartCoroutine(PlayUpgradePulse(node.trapObject.transform));
                        }
                    }
                }

                UpdateUpgradeCostUI(x, y);
            }
        }
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

        List<GridNode> testPath = Pathfinder.Instance.FindPath(startX, startY, goalX, goalY);

        GridManager.Instance.SetCellBlocked(x, y, false);

        return testPath == null;
    }
}