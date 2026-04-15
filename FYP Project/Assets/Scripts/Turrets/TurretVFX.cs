using System.Collections;
using UnityEngine;

public class TurretVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject muzzleFlash;

    [Header("Timing")]
    [SerializeField] private float tracerDuration = 0.08f;
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    private Coroutine tracerRoutine;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
    }

    public void FireTracer(Vector3 targetPosition)
    {
        if (firePoint == null || lineRenderer == null)
        {
            Debug.LogWarning("TurretVFX missing firePoint or lineRenderer.");
            return;
        }

        if (tracerRoutine != null)
            StopCoroutine(tracerRoutine);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        tracerRoutine = StartCoroutine(ShowTracer(targetPosition));
        flashRoutine = StartCoroutine(ShowMuzzleFlash());
    }

    private IEnumerator ShowTracer(Vector3 targetPosition)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, targetPosition);

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
        tracerRoutine = null;
    }

    private IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(muzzleFlashDuration);
            muzzleFlash.SetActive(false);
        }

        flashRoutine = null;
    }
}