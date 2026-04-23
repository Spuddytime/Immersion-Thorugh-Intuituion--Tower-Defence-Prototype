using TMPro;
using UnityEngine;

// Handles updating on-screen UI elements for both desktop and VR
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Desktop UI")]
    public TextMeshProUGUI baseHealthText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI buildModeText;
    public TextMeshProUGUI buildCostText;
    public TextMeshProUGUI upgradeCostText; // ✅ NEW
    public TextMeshProUGUI buildHintText;
    public GameObject gameOverText;

    [Header("VR UI")]
    public TextMeshProUGUI vrBaseHealthText;
    public TextMeshProUGUI vrWaveText;
    public TextMeshProUGUI vrMoneyText;
    public TextMeshProUGUI vrBuildModeText;
    public TextMeshProUGUI vrBuildCostText;
    public TextMeshProUGUI vrUpgradeCostText; // ✅ NEW
    public GameObject vrGameOverText;

    private void Awake()
    {
        Instance = this;

        if (gameOverText != null)
            gameOverText.SetActive(false);

        if (vrGameOverText != null)
            vrGameOverText.SetActive(false);

        if (buildHintText != null)
        {
            buildHintText.text = "1 = Wall    2 = Turret    3 = Trap    4 = Anti-Air";
        }
    }

    public void UpdateBaseHealth(int currentHealth)
    {
        if (baseHealthText != null)
            baseHealthText.text = "Base HP: " + currentHealth;

        if (vrBaseHealthText != null)
            vrBaseHealthText.text = "Base HP: " + currentHealth;
    }

    public void UpdateWave(int currentWave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + currentWave;

        if (vrWaveText != null)
            vrWaveText.text = "Wave: " + currentWave;
    }

    public void UpdateMoney(int currentMoney)
    {
        if (moneyText != null)
            moneyText.text = "Money: " + currentMoney;

        if (vrMoneyText != null)
            vrMoneyText.text = "Money: " + currentMoney;
    }

    public void UpdateBuildMode(string modeName)
    {
        if (buildModeText != null)
            buildModeText.text = "Mode: " + modeName;

        if (vrBuildModeText != null)
            vrBuildModeText.text = "Mode: " + modeName;
    }

    public void UpdateBuildCost(int cost)
    {
        if (buildCostText != null)
            buildCostText.text = "Build Cost: " + cost;

        if (vrBuildCostText != null)
            vrBuildCostText.text = "Build Cost: " + cost;
    }

    public void UpdateUpgradeCost(int cost)
    {
        string text = cost > 0 ? "Upgrade: " + cost : "Upgrade: -";

        if (upgradeCostText != null)
            upgradeCostText.text = text;

        if (vrUpgradeCostText != null)
            vrUpgradeCostText.text = text;
    }

    public void ShowGameOver()
    {
        if (gameOverText != null)
            gameOverText.SetActive(true);

        if (vrGameOverText != null)
            vrGameOverText.SetActive(true);
    }
}