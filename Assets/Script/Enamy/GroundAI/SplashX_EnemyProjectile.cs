using UnityEngine;

public class SplashX_EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;           // ความเร็วกระสุน
    public int damage = 15;             // ดาเมจที่ทำกับผู้เล่น
    public float lifeTime = 3f;         // ระยะเวลาทำลายตัวเองถ้าไม่ชนอะไร

    [Header("Visual & Effects")]
    public float spinSpeed = 0f;        // ถ้าอยากให้กระสุนหมุนติ้วๆ ใส่เลขเข้าไป (เช่น 360) ถ้ากระสุนปืนธรรมดาใส่ 0
    public GameObject hitVFX;           // เอฟเฟกต์ตอนกระสุนแตก

    [Header("Target Layers")]
    public LayerMask groundLayer;       // เลเยอร์ของพื้น/กำแพง (ชนแล้วแตก)

    private Rigidbody2D rb;
    private float moveDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ทิศทางพุ่ง: อิงจากหน้าของ FirePoint ตอนเสกออกมา
        moveDirection = Mathf.Sign(transform.right.x);

        if (rb != null)
        {
            // พุ่งตรงไปข้างหน้า (ซ้ายหรือขวาตามทิศที่หัน)
            rb.linearVelocity = transform.right * speed;
        }

        // ตั้งเวลาทำลายตัวเอง
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 🌀 หมุนภาพกระสุน (ถ้าตั้งค่า spinSpeed ไว้)
        if (spinSpeed > 0)
        {
            transform.Rotate(0f, 0f, -moveDirection * spinSpeed * Time.deltaTime);
        }
    }

    // เมื่อกระสุนพุ่งไปชนอะไรสักอย่าง
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. เช็คว่าชน "ผู้เล่น" ไหม
        if (collision.CompareTag("Player"))
        {
            SplashX_PlayerStats playerStats = collision.GetComponent<SplashX_PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage); // ทำดาเมจใส่เคียว
            }
            TriggerHitEffect();
        }
        // 2. เช็คว่าชน "กำแพง/พื้น" ไหม
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            TriggerHitEffect();
        }
    }

    void TriggerHitEffect()
    {
        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}