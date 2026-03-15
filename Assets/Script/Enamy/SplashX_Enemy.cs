using UnityEngine;

public class SplashX_Enemy : MonoBehaviour
{
    // Note: Always place [Header] attributes inside the class and above the variables.
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        // Initialize health at the start of the game
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Reduces current health and checks for death condition.
    /// </summary>
    /// <param name="damage">The amount of health to subtract.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage! Remaining HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Add death effects or drop items here before destruction
        Debug.Log("Enemy has been defeated!");
        Destroy(gameObject);
    }
}