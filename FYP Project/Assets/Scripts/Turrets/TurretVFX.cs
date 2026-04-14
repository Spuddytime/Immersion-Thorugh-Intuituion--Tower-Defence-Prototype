using System.Collections;
using UnityEngine;

public class TurretVFX : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private float tracerDuration = 0.05f;

    private void Awake()
    {
        lineRenderer.enabled = false;
    }

    public void FireTracer(Vector3 targetPosition)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTracer(targetPosition));
    }

    private IEnumerator ShowTracer(Vector3 targetPosition)
    {
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, targetPosition);

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
    }
}