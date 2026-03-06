using UnityEngine;

// Handles player input for placing and removing buildable objects using the mouse
public class MouseBuilder : MonoBehaviour
{
    // Camera used to generate the ray from the mouse
    public Camera mainCamera;

    // Object that will be placed on the grid
    public GameObject wallPrefab;

    // Visual object used to show the currently selected grid cell
    public Transform cellHighlight;

    // Layer used for the ground so the raycast only hits the floor
    public LayerMask groundLayer;

    void Update()
    {
        UpdateHighlight();

        // Left click places a wall
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }

        // Right click removes a wall
        if (Input.GetMouseButtonDown(1))
        {
            TryRemove();
        }
    }

    // Moves the highlight object to the grid cell under the mouse
    void UpdateHighlight()
    {
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

    // Attempts to place a wall where the mouse is pointing
    void TryPlace()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                GridManager.Instance.PlaceObject(x, y, wallPrefab);
            }
        }
    }

    // Attempts to remove a wall from the clicked cell
    void TryRemove()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            if (GridManager.Instance.GetXY(hit.point, out int x, out int y))
            {
                GridManager.Instance.ClearCell(x, y);
            }
        }
    }
}