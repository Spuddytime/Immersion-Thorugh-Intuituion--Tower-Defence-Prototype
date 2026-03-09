using UnityEngine;

// Simple single-use spike trap that damages the first valid enemy that touches it
public class SpikeTrap : MonoBehaviour
{
    public int damage = 2;
    public TurretTargetType targetType = TurretTargetType.GroundOnly;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy == null)
            return;

        if (!CanTarget(enemy))
            return;

        hasTriggered = true;

        enemy.TakeDamage(damage);

        Debug.Log(gameObject.name + " triggered on " + enemy.name);

        Destroy(gameObject);
    }

    bool CanTarget(EnemyHealth enemy)
    {
        switch (targetType)
        {
            case TurretTargetType.GroundOnly:
                return enemy.unitType == EnemyUnitType.Ground;

            case TurretTargetType.FlyingOnly:
                return enemy.unitType == EnemyUnitType.Flying;

            case TurretTargetType.Both:
                return true;
        }

        return false;
    }
}