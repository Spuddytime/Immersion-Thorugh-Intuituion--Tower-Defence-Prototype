using UnityEngine;

// Handles the player's base health and lose condition
public class BaseHealth : MonoBehaviour
{
    public int maxHealth = 10;

    private int currentHealth;
    private bool isGameOver = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBaseHealth(currentHealth);
        }

        Debug.Log("Base Health: " + currentHealth);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Base took damage. Current Health: " + currentHealth);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBaseHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        Time.timeScale = 0f;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}