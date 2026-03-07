using TMPro;
using UnityEngine;

// Handles updating on-screen UI elements
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI baseHealthText;
    public TextMeshProUGUI waveText;
    public GameObject gameOverText;

    private void Awake()
    {
        Instance = this;

        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }
    }

    public void UpdateBaseHealth(int currentHealth)
    {
        if (baseHealthText != null)
        {
            baseHealthText.text = "Base HP: " + currentHealth;
        }
    }

    public void UpdateWave(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }
    }
}