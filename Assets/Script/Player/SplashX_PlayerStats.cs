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
        if (isDead || isInvincible) return;

        // 🔥 แทรกระบบลดดาเมจตอนเปิดโหมดบ้าคลั่ง (Berserk)
        SplashX_PerkSystem perkSystem = GetComponent<SplashX_PerkSystem>();
        if (perkSystem != null && perkSystem.isBerserk)
        {
            // เอาดาเมจมาคูณเปอร์เซ็นต์ลดทอน แล้วปัดเศษเป็นจำนวนเต็ม
            damage = Mathf.RoundToInt(damage * perkSystem.berserkDamageReduction);
            Debug.Log("🛡️ โหมดบ้าคลั่งทำงาน! ลดดาเมจเหลือ: " + damage);
        }

        currentHp -= damage;
        Debug.Log("Player hit! Remaining HP: " + currentHp);

        // ตัดสินใจว่าจะเล่นท่าเจ็บ หรือ ท่าตาย
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
    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }

        Debug.Log("Healed! Current HP: " + currentHp);
    }

    // 💀 ระบบตาย (เปิดร่างกระดูกก่อน แล้วบังคับกระชากเข้าท่า Death ทันที!)
    private IEnumerator DeathRoutine()
    {
        isDead = true;

        // 🔥 1. สลับร่างก่อนเป็นอย่างแรกสุด! ตามที่คุณเซฟบอกเลยครับ
        if (movement != null)
        {
            if (movement.fbfAttackModel != null) movement.fbfAttackModel.SetActive(false);
            if (movement.boneModel != null) movement.boneModel.SetActive(true);

            // 🔥 2. พอร่างกระดูกเปิดปุ๊บ บังคับยัดข้อมูลหลอกสมองมันทันที!
            if (movement.boneAnim != null)
            {
                // หลอกมันว่า "เหยียบพื้นแล้ว และไม่ได้ขยับ" (ปิดประตูท่า Fall 100%)
                movement.boneAnim.SetBool("isGrounded", true);
                movement.boneAnim.SetFloat("yVelocity", 0f);
                movement.boneAnim.SetFloat("Speed", 0f);

                // ล้างคำสั่งเจ็บ/ฟันดาบ ที่อาจจะค้างอยู่
                movement.boneAnim.ResetTrigger("Hurt");

                // 🔥 3. ไม้ตายขั้นสุด: เลิกใช้ Trigger แล้วใช้คำสั่ง Play บังคับเล่นท่าตายเดี๋ยวนั้นเลย!
                // (คำเตือน: ในหน้าต่าง Animator กล่องท่าตายต้องชื่อ "Death" เป๊ะๆ นะครับ ถ้าชื่ออื่นให้แก้ตรงนี้ให้ตรงกัน)
                movement.boneAnim.Play("Death", -1, 0f);
            }
        }

        // 4. ล็อกการเดินและปิดฟิสิกส์ (ทำทีหลังสุด)
        if (movement != null)
        {
            movement.StopAllCoroutines();
            movement.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // 5. รอแอนิเมชันตายจบ 
        yield return new WaitForSeconds(2f);

        // 6. ส่งเรื่องให้ GameManager พาไปจุดเกิด
        if (SplashX_GameManager.instance != null)
        {
            SplashX_GameManager.instance.HandlePlayerDeath();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 🌟 ระบบชุบชีวิต
    public void Revive()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;
        isDead = false;
        isInvincible = false;

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (movement != null)
        {
            movement.enabled = true;
            movement.ResetAllStatesForRevive(); // ฟังก์ชันนี้จะจัดการล้างสมองและคืนค่าแรงโน้มถ่วงให้เอง
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

    public void Bounce(float force)
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }
}