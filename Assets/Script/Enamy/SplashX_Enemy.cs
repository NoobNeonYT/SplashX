using UnityEngine;

public class SplashX_Enemy : MonoBehaviour
{
    // วาง [Header] ไว้เหนือตัวแปรเท่านั้น ห้ามวางนอก class
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage! HP left: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy Died!");
        Destroy(gameObject);
    }
}