using System.Collections;
using UnityEngine;

public class SplashX_Enemy : MonoBehaviour
{
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
        // Add death effects or drop items here before destruction
        Debug.Log("Enemy has been defeated!");
        Destroy(gameObject);
    }
}