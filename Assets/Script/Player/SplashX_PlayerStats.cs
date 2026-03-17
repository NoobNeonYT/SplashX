using System.Collections;
using UnityEngine;

public class SplashX_PlayerStats : MonoBehaviour
{
    [Header("Health System")]
    public int maxHp = 100;
    public int currentHp;
    public float iFrameDuration = 1f; // ระยะเวลาอมตะหลังโดนตี
    private bool isInvincible = false;
    private bool isDead = false; // เพิ่มตัวแปรเช็คว่าตายหรือยัง

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 20f;
    public float staminaRegenDelay = 1f;
    private float regenTimer;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    private Color originalColor;

    private SplashX_PlayerMovement movement;
    private Rigidbody2D rb;

    void Start()
    {
        movement = GetComponent<SplashX_PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        // 🔗 เชื่อมระบบ Checkpoint: ดึงตำแหน่งเกิดจาก GameManager ตอนเริ่มเกม
        if (SplashX_GameManager.instance != null)
        {
            transform.position = SplashX_GameManager.instance.GetRespawnPosition(transform.position);
        }

        currentHp = maxHp;
        currentStamina = maxStamina;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead) return; // ตายแล้วไม่ต้องฟื้นสตามิน่า
        HandleStaminaRegen();
    }

    // --- Health Logic ---
    public void TakeDamage(int damage)
    {
        // ข้ามการทำดาเมจถ้าตายแล้ว หรือติดสถานะอมตะอยู่
        if (isDead || isInvincible) return;

        currentHp -= damage;
        Debug.Log("Player hit! Remaining HP: " + currentHp);

        if (currentHp <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
        else
        {
            StartCoroutine(DamageRoutine());
            if (movement != null)
            {
                movement.TriggerHurt();
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return; // ตายแล้วฮีลไม่ขึ้น

        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp; // ล็อกไม่ให้เลือดล้นหลอด
        }

        Debug.Log("Healed! Current HP: " + currentHp);

        // 🌟 ถ้าอยากให้ตัวกระพริบสีเขียวตอนฮีล ก็เพิ่ม Coroutine คล้ายๆ ตอนโดนตีได้เลยครับ
    }

    // 💀 ระบบตาย (แทนที่ฟังก์ชัน Die เดิม)
    private IEnumerator DeathRoutine()
    {
        isDead = true;
        Debug.Log("Player has fallen!");

        // 1. ล็อกการควบคุมทั้งหมด
        if (movement != null)
        {
            movement.StopAllCoroutines();
            movement.enabled = false;
        }

        // 2. หยุดความเร็วร่วงหล่น
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 3. สลับร่างและเล่นท่าตาย
        if (movement != null)
        {
            if (movement.fbfAttackModel != null) movement.fbfAttackModel.SetActive(false);
            if (movement.boneModel != null) movement.boneModel.SetActive(true);

            if (movement.boneAnim != null)
            {
                movement.boneAnim.SetTrigger("Death");
            }
        }

        // 4. รอแอนิเมชันตายจบ (ปรับเวลาได้ตามความยาวท่าตาย)
        yield return new WaitForSeconds(2f);

        // 5. ส่งเรื่องให้ GameManager พาไปจุดเกิด หรือ รีเซ็ตด่าน
        if (SplashX_GameManager.instance != null)
        {
            SplashX_GameManager.instance.HandlePlayerDeath();
        }
        else
        {
            // กันเหนียวกรณีไม่ได้วาง GameManager ไว้ในด่าน
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 🌟 ระบบชุบชีวิต (โดนเรียกใช้จาก GameManager)
    public void Revive()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;
        isDead = false;
        isInvincible = false;

        // คืนสีเดิมเผื่อตายตอนตัวแดง
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (movement != null)
        {
            movement.enabled = true; // ปลดล็อกให้เดินได้
            if (movement.boneAnim != null)
            {
                movement.boneAnim.Play("Player_idle");
            }
        }
    }

    // Visual feedback and Invincibility frames
    IEnumerator DamageRoutine()
    {
        isInvincible = true;

        if (spriteRenderer != null) spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(iFrameDuration - 0.1f);
        isInvincible = false;
    }

    // --- Stamina Logic ---
    public bool UseStamina(float amount)
    {
        if (isDead) return false;

        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = staminaRegenDelay;
            return true;
        }

        Debug.Log("Out of Stamina!");
        return false;
    }

    void HandleStaminaRegen()
    {
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }
}