using System.Collections;
using UnityEngine;

public class SplashX_Enemy : MonoBehaviour
{
    [Header("Drop System")]
    public GameObject healthPickupPrefab; // ลาก Prefab ไอเทมฮีลมาใส่ช่องนี้
    [Range(0f, 1f)]
    public float dropChance = 0.10f; // โอกาส 10% (0.10 = 10%, 1.0 = 100%)
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Hit Feedback (การตอบสนองตอนโดนตี)")]
    public bool isBoss = false;           // ติ๊กถูกถ้าตัวนี้คือบอส (บอสจะไม่ชะงัก)
    public GameObject hitSparkPrefab;     // ลาก Prefab สะเก็ดไฟมาใส่ช่องนี้
    public float stunDuration = 0.2f;     // เวลาชะงัก (วินาที)

    // ตัวแปรเช็คสถานะชะงัก เอาไปขวางระบบเดินของศัตรู
    public bool isStunned = false;
    private Rigidbody2D rb;

    void Start()
    {
        // Initialize health at the start of the game
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 🔥 ถ้ากำลังชะงักอยู่ ให้หยุดทำงานบรรทัดล่างๆ ทันที
        // (เอาไว้ดักระบบเดิน/โจมตีของศัตรูในอนาคต)
        if (isStunned) return;

        // ... โค้ด AI เดินหรือโจมตีของมอนสเตอร์ในอนาคต เอามาใส่ต่อจากบรรทัดนี้ได้เลย ...
    }

    /// <summary>
    /// Reduces current health and checks for death condition.
    /// </summary>
    /// <param name="damage">The amount of health to subtract.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage! Remaining HP: " + currentHealth);

        // 1. เสกสะเก็ดไฟตรงตำแหน่งศัตรู (ถ้าวาง Prefab ไว้)
        if (hitSparkPrefab != null)
        {
            Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 2. ทำให้ชะงัก (ถ้าเลือดยังไม่หมด, ไม่ใช่บอส และไม่ได้กำลังชะงักอยู่)
            if (!isBoss && !isStunned)
            {
                StartCoroutine(HitStunRoutine());
            }
        }
    }

    private IEnumerator HitStunRoutine()
    {
        isStunned = true;

        // หยุดความเร็วของศัตรูให้เป็น 0 ทันที (เวลาโดนตีจะได้ไม่ไถลเข้ามาหาเรา)
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
        }

        // รอเวลาเสี้ยววินาทีตามที่ตั้งไว้
        yield return new WaitForSeconds(stunDuration);

        // ปลดล็อกให้ศัตรูกลับมาขยับได้ต่อ
        isStunned = false;
    }

    void Die()
    {
        Debug.Log("Enemy has been defeated!");

        // 🔥 1. ระบบแจกแต้ม Perk! (เฉพาะมอนสเตอร์ธรรมดา)
        if (!isBoss)
        {
            int randomPerk = Random.Range(10, 21);
            SplashX_PerkSystem perkSystem = FindFirstObjectByType<SplashX_PerkSystem>();
            if (perkSystem != null)
            {
                perkSystem.AddPerkPoints(randomPerk);
            }
        }

        // 🎲 2. ทอยเต๋าสุ่มดรอปไอเทมฮีล
        if (!isBoss && healthPickupPrefab != null)
        {
            if (Random.value <= dropChance)
            {
                Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            }
        }

        // 🔥 3. แทนที่จะ Destroy ทันที ให้เรียก Coroutine หน่วงเวลาแทน
        StartCoroutine(DestroyAfterDeathAnim());
    }

    // ระบบจัดการศพหลังตาย
    private IEnumerator DestroyAfterDeathAnim()
    {
        // 1. ปิดกล่องชนทันที
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. ปิดฟิสิกส์ไม่ให้ไหลตกแมพ
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3. ปิดสคริปต์ AI ทุกตัว (Animator รอดอยู่แล้วเพราะมันไม่ใช่ MonoBehaviour)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) // ปิดทุกสคริปต์ที่ไม่ใช่สคริปต์นี้
            {
                script.enabled = false;
            }
        }

        // 4. รอเวลาให้เล่นแอนิเมชันตายจนจบ (ปรับตัวเลขตามชอบ)
        yield return new WaitForSeconds(2f);

        // 5. ลบศพทิ้งออกจากฉาก
        Destroy(gameObject);
    }
}