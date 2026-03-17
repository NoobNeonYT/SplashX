using System.Collections;
using UnityEngine;

public class SplashX_HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 20;
    public GameObject pickupEffect;

    [Header("Drop Settings (เด้ง & ดีเลย์)")]
    public float dropForceX = 3f;
    public float dropForceY = 6f;
    public float pickupDelay = 1.2f; // หน่วงเวลาให้เกิดความอยากก่อนเก็บ
    private bool canPickup = false;

    [Header("Hover & Spin (ลอยและหมุน)")]
    public float spinSpeed = 150f;
    public float baseOffset = 0.5f;   // 🔥 ดันภาพขึ้นจากพื้น (ปรับเลขนี้เพื่อแก้จมดิน)
    public float bounceHeight = 0.2f; // 🔥 ระยะเด้งขึ้นลง (Sine Wave)
    public float hoverSpeed = 4f;     // ความเร็วตอนลอยขึ้นลง

    [Header("Ground Check (กันสะดุด)")]
    public LayerMask groundLayer; // ลากเลเยอร์พื้นมาใส่ช่องนี้
    public float groundCheckDistance = 0.3f; // ความยาวเลเซอร์เช็คพื้น (ดึงเส้นสีแดงในหน้า Scene ให้แตะพื้นพอดี)

    private bool isSettled = false;
    private float settledY;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            float randomX = Random.Range(-dropForceX, dropForceX);
            rb.AddForce(new Vector2(randomX, dropForceY), ForceMode2D.Impulse);
        }

        Invoke("EnablePickup", pickupDelay);
    }

    void EnablePickup()
    {
        canPickup = true;
    }

    void Update()
    {
        // 1. ระบบเช็คพื้นระหว่างที่ไอเทมกำลังร่วง
        if (!isSettled)
        {
            // เช็คเฉพาะตอนที่ไอเทมกำลังตก (แกน Y ติดลบ) จะได้ไม่บั๊กตอนเพิ่งเด้งขึ้นฟ้า
            if (rb != null && rb.linearVelocity.y <= 0)
            {
                // ยิงเลเซอร์เช็คพื้นลงไปด้านล่าง
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

                if (hit.collider != null)
                {
                    isSettled = true;

                    // ปิดฟิสิกส์แรงโน้มถ่วงทิ้งทันทีที่แตะพื้น
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;

                    // 🔥 ยึดตำแหน่งแกน Y ที่เลเซอร์ชนพื้น + ดันขึ้นด้วย baseOffset กันภาพจม
                    settledY = hit.point.y + baseOffset;
                }
            }
        }
        // 2. ท่าเต้นเรียกร้องความสนใจ (จะทำงานก็ต่อเมื่อตกถึงพื้นแล้ว)
        else
        {
            // หมุนแกน Y ภาพจะพลิกและดูแบนลงเหมือนเหรียญหมุน
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

            // 🔥 เด้งขึ้นลงเบาๆ ด้วยสมการ Sine Wave ที่แยกความสูงการเด้งไว้แล้ว
            float newY = settledY + (Mathf.Sin(Time.time * hoverSpeed) * bounceHeight);
            transform.position = new Vector2(transform.position.x, newY);
        }
    }

    // เอาไว้วาดเส้นสีแดงให้เห็นระยะเช็คพื้นในหน้า Scene (ตอนยังไม่กด Play)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

    // ฟังก์ชันนี้เช็คตอนผู้เล่นเดินมาชนกล่องเพื่อเก็บ
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;

        if (other.CompareTag("Player"))
        {
            SplashX_PlayerStats playerStats = other.GetComponent<SplashX_PlayerStats>();
            if (playerStats != null && playerStats.currentHp < playerStats.maxHp)
            {
                playerStats.Heal(healAmount);
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}