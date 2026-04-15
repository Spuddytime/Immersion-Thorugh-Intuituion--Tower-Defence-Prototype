using UnityEngine;

public class TestFire : MonoBehaviour
{
    [SerializeField] private TurretVFX turretVFX;
    [SerializeField] private Transform target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (turretVFX != null && target != null)
            {
                Vector3 aimPoint = target.position + Vector3.up * 0.5f;
                turretVFX.FireTracer(aimPoint);
            }
        }
    }
}