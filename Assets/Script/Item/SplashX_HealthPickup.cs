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
    public float hoverHeight = 0.2f; // ลอยเหนือพื้นแค่นี้
    public float hoverSpeed = 4f;    // ความเร็วตอนลอยขึ้นลง

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
        // ท่าเต้นเรียกร้องความสนใจ (จะทำงานก็ต่อเมื่อตกถึงพื้นแล้ว)
        if (isSettled)
        {
            // 🔥 หมุนแกน Y แทน (ใส่ค่าตรงกลาง) ภาพจะพลิกและดูแบนลงเหมือนเหรียญหมุน
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

            // เด้งขึ้นลงเบาๆ ด้วยสมการ Sine Wave
            float newY = settledY + (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
            transform.position = new Vector2(transform.position.x, newY);
        }
    }

    // ฟังก์ชันนี้เช็คตอนไอเทมหล่นกระแทกพื้น
    void OnCollisionEnter2D(Collision2D collision)
    {
        // เช็คว่าตกถึง "พื้น" หรือยัง (อะไรก็ได้ที่ไม่ใช่ผู้เล่น และต้องชนจากด้านล่าง)
        if (!isSettled && !collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            {
                isSettled = true;
                if (rb != null)
                {
                    // พอถึงพื้นปุ๊บ สั่งปิดระบบแรงโน้มถ่วง (จะได้ไม่หล่นทะลุพื้นตอนลอย) และเบรกความเร็วให้เป็น 0
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                // เซฟตำแหน่งแกน Y ตรงพื้นเอาไว้เป็นจุดกึ่งกลางตอนลอยขึ้นลง
                settledY = transform.position.y + hoverHeight;
            }
        }
    }

    // ฟังก์ชันนี้เช็คตอนผู้เล่นเดินมาชนกล่องเพื่อเก็บ
    void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้ายังไม่หมดเวลาดีเลย์ ห้ามเก็บเด็ดขาด! ทิ้งให้ผู้เล่นดูน้ำลายไหลไปก่อน
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