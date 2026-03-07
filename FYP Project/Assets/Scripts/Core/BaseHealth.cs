using UnityEngine;

// Handles the player's base health and lose condition
public class BaseHealth : MonoBehaviour
{
    public int maxHealth = 10;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log("Base Health: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Base took damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over");
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}