using UnityEngine;
using System.Collections;

public class SplashX_PlayerStats : MonoBehaviour
{
    [Header("Health System")]
    public int maxHp = 100;
    public int currentHp;
    public float iFrameDuration = 1f; // ระยะเวลาอมตะหลังโดนตี
    private bool isInvincible = false;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 20f; // ความเร็วในการฟื้นฟู
    public float staminaRegenDelay = 1f; // ดีเลย์ก่อนเริ่มฟื้นฟู (หลังใช้สกิล)
    private float regenTimer;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    private Color originalColor;

    void Start()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;

        // หา SpriteRenderer อัตโนมัติเพื่อเอาไว้ทำเอฟเฟกต์กระพริบแดง
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        HandleStaminaRegen();
    }

    // --- ระบบเลือด (HP) ---
    public void TakeDamage(int damage)
    {
        if (isInvincible) return; // ถ้าติดอมตะอยู่ ดาเมจจะไม่เข้า

        currentHp -= damage;
        Debug.Log("โดนโจมตี! เลือดเหลือ: " + currentHp);

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
        Debug.Log("Kyo ตายแล้ว!");
        // เดี๋ยวเราค่อยมาเขียนระบบ Game Over หรือโหลด Checkpoint ตรงนี้ครับ
    }

    // เอฟเฟกต์กระพริบแดง และ อมตะชั่วคราว (I-Frame)
    IEnumerator DamageRoutine()
    {
        isInvincible = true;

        if (spriteRenderer != null) spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(iFrameDuration - 0.1f);
        isInvincible = false;
    }

    // --- ระบบความเหนื่อย (Stamina) ---
    // ฟังก์ชันนี้จะส่งค่ากลับเป็น true ถ้ามี Stamina พอใช้ และ false ถ้าไม่พอ
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = staminaRegenDelay; // รีเซ็ตเวลาดีเลย์ ไม่ให้มันฟื้นฟูทันทีตอนกำลังแด้ช
            return true;
        }

        Debug.Log("Stamina หมด!");
        return false;
    }

    // ฟื้นฟู Stamina อัตโนมัติ
    void HandleStaminaRegen()
    {
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime; // นับถอยหลังดีเลย์
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }
}