using UnityEngine;

// Simple turret that finds a valid target, rotates toward it, and deals instant damage
public class Turret : MonoBehaviour
{
    public float range = 4f;
    public float fireRate = 1f;
    public int damage = 1;
    public TurretTargetType targetType = TurretTargetType.GroundOnly;

    private float fireCooldown = 0f;
    private EnemyHealth currentTarget;

    void Update()
    {
        FindTarget();

        if (currentTarget != null)
        {
            RotateTowardsTarget();

            fireCooldown -= Time.deltaTime;

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            fireCooldown = Mathf.Max(fireCooldown - Time.deltaTime, 0f);
        }
    }

    void FindTarget()
    {
        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        EnemyHealth closestValidTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy == null)
                continue;

            if (!CanTarget(enemy))
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= range && distance < closestDistance)
            {
                closestDistance = distance;
                closestValidTarget = enemy;
            }
        }

        currentTarget = closestValidTarget;
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

    void RotateTowardsTarget()
    {
        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
        }
    }

    void Fire()
    {
        if (currentTarget == null)
            return;

        Debug.Log(gameObject.name + " fired at " + currentTarget.name);
        currentTarget.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}