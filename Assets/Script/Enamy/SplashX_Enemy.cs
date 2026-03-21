using System.Collections;
using UnityEngine;

public class SplashX_Enemy : MonoBehaviour
{
    [Header("Drop System")]
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.10f;

    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Hit Feedback (การตอบสนองตอนโดนตี)")]
    public bool isBoss = false;            // ติ๊กถูกถ้าตัวนี้คือบอส (บอสจะไม่ชะงัก)
    public bool isBossDontDestroy = false; // 🔥 ติ๊กถูกถ้าเป็นป้อมบอส หรือบอสที่ห้ามลบศพทิ้ง
    public GameObject hitSparkPrefab;
    public float stunDuration = 0.2f;

    public bool isStunned = false;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isStunned) return;

        // ... โค้ด AI เดินหรือโจมตีของมอนสเตอร์ในอนาคต เอามาใส่ต่อจากบรรทัดนี้ได้เลย ...
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took damage! Remaining HP: " + currentHealth);

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
            // ถ้าเลือดยังไม่หมด, ไม่ใช่บอส(ตัวเดิน) และไม่ได้กำลังชะงักอยู่
            if (!isBoss && !isStunned)
            {
                StartCoroutine(HitStunRoutine());
            }
        }
    }

    private IEnumerator HitStunRoutine()
    {
        isStunned = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
    }

    void Die()
    {
        Debug.Log("Enemy has been defeated!");

        // 1. ระบบแจกแต้ม Perk! (เฉพาะมอนสเตอร์ธรรมดา)
        if (!isBoss && !isBossDontDestroy)
        {
            int randomPerk = Random.Range(10, 21);
            SplashX_PerkSystem perkSystem = FindFirstObjectByType<SplashX_PerkSystem>();
            if (perkSystem != null)
            {
                perkSystem.AddPerkPoints(randomPerk);
            }
        }

        // 2. ทอยเต๋าสุ่มดรอปไอเทมฮีล
        if (!isBoss && !isBossDontDestroy && healthPickupPrefab != null)
        {
            if (Random.value <= dropChance)
            {
                Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            }
        }

        // 🔥 3. ถ้าเป็นชิ้นส่วนบอสที่ห้ามลบทิ้ง ให้หยุดการทำงานตรงนี้เลย!
        // ปล่อยให้สคริปต์ SplashX_BossNode จัดการเปลี่ยนรูปภาพดับไฟเอง
        if (isBossDontDestroy) return;

        // 4. ถ้าเป็นมอนสเตอร์ปกติ ค่อยเรียก Coroutine จัดการศพ
        StartCoroutine(DestroyAfterDeathAnim());
    }

    private IEnumerator DestroyAfterDeathAnim()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}