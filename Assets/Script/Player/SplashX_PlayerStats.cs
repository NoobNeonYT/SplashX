using UnityEngine;
using System.Collections;

public class SplashX_PlayerStats : MonoBehaviour
{
    [Header("Health System")]
    public int maxHp = 100;
    public int currentHp;
    public float iFrameDuration = 1f; // Invincibility period after taking damage
    private bool isInvincible = false;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 20f; // Points per second
    public float staminaRegenDelay = 1f; // Cooldown before regen starts after use
    private float regenTimer;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    private Color originalColor;

    void Start()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;

        // Auto-assign SpriteRenderer if not set in Inspector
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        HandleStaminaRegen();
    }

    // --- Health Logic ---
    public void TakeDamage(int damage)
    {
        // Skip damage if player is currently in I-Frames
        if (isInvincible) return;

        currentHp -= damage;
        Debug.Log("Player hit! Remaining HP: " + currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageRoutine());
        }
    }

    void Die()
    {
        Debug.Log("Player has fallen!");
        // TODO: Implement Game Over sequence or Checkpoint reload
    }

    // Visual feedback and Invincibility frames
    IEnumerator DamageRoutine()
    {
        isInvincible = true;

        // Brief flash to indicate damage
        if (spriteRenderer != null) spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        // Maintain invincibility for the remaining duration
        yield return new WaitForSeconds(iFrameDuration - 0.1f);
        isInvincible = false;
    }

    // --- Stamina Logic ---

    /// <summary>
    /// Consumes stamina if enough is available.
    /// </summary>
    /// <returns>True if action is successful, False if not enough stamina.</returns>
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = staminaRegenDelay; // Reset regen delay upon consumption
            return true;
        }

        Debug.Log("Out of Stamina!");
        return false;
    }

    // Automatically recover stamina over time
    void HandleStaminaRegen()
    {
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime; // Countdown the delay timer
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }
}