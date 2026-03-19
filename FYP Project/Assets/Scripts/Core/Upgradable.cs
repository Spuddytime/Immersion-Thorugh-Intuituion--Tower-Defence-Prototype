using UnityEngine;

// Generic upgrade component for simple level-based upgrades
public class Upgradeable : MonoBehaviour
{
    public int currentLevel = 1;
    public int maxLevel = 3;

    public int[] upgradeCosts = { 15, 25 };

    public Turret turret;
    public SpikeTrap spikeTrap;

    public bool CanUpgrade()
    {
        return currentLevel < maxLevel && GetUpgradeCost() > 0;
    }

    public int GetUpgradeCost()
    {
        int costIndex = currentLevel - 1;

        if (upgradeCosts == null || costIndex < 0 || costIndex >= upgradeCosts.Length)
            return 0;

        return upgradeCosts[costIndex];
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade())
            return false;

        int cost = GetUpgradeCost();

        if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(cost))
        {
            Debug.Log("Not enough money to upgrade.");
            return false;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.SpendMoney(cost);
        }

        currentLevel++;

        ApplyUpgrade();

        Debug.Log(gameObject.name + " upgraded to level " + currentLevel);
        return true;
    }

    void ApplyUpgrade()
    {
        if (turret != null)
        {
            turret.damage += 1;
            turret.range += 0.5f;
            turret.fireRate += 0.25f;
        }

        if (spikeTrap != null)
        {
            spikeTrap.damage += 1;
        }
    }
}