using UnityEngine;

public class SplashX_Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;           // ความเร็วกระสุน
    public int damage = 15;             // ดาเมจ
    public float lifeTime = 2f;         // ระยะเวลาทำลายตัวเอง (ถ้าไม่ชนอะไรเลย)

    [Header("Visual & Effects")]
    public float spinSpeed = 720f;      // ความเร็วในการหมุนกลิ้ง (องศาต่อวินาที)
    public GameObject hitVFX;           // เอฟเฟกต์ตอนกระสุนชนเป้าหมาย

    [Header("Target Layers")]
    public LayerMask enemyLayer;        // เลเยอร์ของศัตรู
    public LayerMask groundLayer;       // เลเยอร์ของพื้นและกำแพง (ชนแล้วแตกเหมือนกัน)

    private Rigidbody2D rb;
    private float moveDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ทิศทางพุ่ง: ดึงค่าหน้าหันของจุดยิง (attackPoint) มาคำนวณ
        // ถ้า transform.right.x ติดลบ แปลว่ากำลังหันซ้าย
        moveDirection = Mathf.Sign(transform.right.x);

        if (rb != null)
        {
            // สั่งให้พุ่งไปข้างหน้าตามทิศที่หันอยู่
            rb.linearVelocity = transform.right * speed;
        }

        // ตั้งเวลาทำลายตัวเองกันขยะล้นฉาก
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 🌀 สั่งให้ภาพหมุนกลิ้งรอบแกน Z 
        // (คูณ moveDirection เพื่อให้เวลาหันซ้ายหรือขวา มันกลิ้งไปในทิศทางที่ถูกต้อง ไม่กลิ้งถอยหลัง)
        transform.Rotate(0f, 0f, -moveDirection * spinSpeed * Time.deltaTime);
    }

    // เมื่อกระสุนพุ่งไปชนอะไรสักอย่าง
    void OnTriggerEnter2D(Collider2D collision)
    {
        // เช็คว่าชน "ศัตรู" ไหม (ใช้ Layer Check)
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            SplashX_Enemy enemy = collision.GetComponent<SplashX_Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            TriggerHitEffect();
        }
        // หรือถ้าชน "กำแพง/พื้น" ก็ทำลายทิ้งเหมือนกัน (กระสุนจะได้ไม่ทะลุดินไปไกล)
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            TriggerHitEffect();
        }
    }

    void TriggerHitEffect()
    {
        // เสกเอฟเฟกต์กระสุนแตก (ถ้ามี)
        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }

        // ทำลายกระสุนทิ้ง
        Destroy(gameObject);
    }
}