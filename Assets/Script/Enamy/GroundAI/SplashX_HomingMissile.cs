using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SplashX_HomingMissile : MonoBehaviour
{
    [Header("Missile Stages")]
    [Tooltip("เวลา (วินาที) ที่มิสไซล์จะบินขึ้นฟรีก่อนจะเริ่มเลี้ยวหาผู้เล่น")]
    public float freeFlightDuration = 0.5f;

    [Header("Initial Boost Settings (ตอนพุ่งออก)")]
    [Tooltip("แรงผลักขึ้นตรงๆ")]
    public float upwardForce = 8f;
    [Tooltip("แรงส่งไปข้างหน้า (ตามทิศที่มอนสเตอร์หัน)")]
    public float forwardForce = 3f;

    [Header("Homing Settings (ตอนติดตาม)")]
    public float homingSpeed = 10f;
    public float rotateSpeed = 250f; // ความเร็วในการหมุนหัวหาเป้าหมาย
    [Tooltip("ชดเชยองศาภาพจรวด (ถ้าภาพเดิมหันขวาอยู่แล้ว ใส่ 0)")]
    public float angleOffset = 0f;
    public float targetOffsetY = 1.0f;

    [Header("Stats & Lifetime")]
    public int damage = 15;
    public float lifeTime = 5f;

    [Header("Effects & Layers")]
    public GameObject explosionVFX; // Prefab Particle ระเบิด
    public AudioClip explosionSound; // เสียงระเบิด
    public LayerMask groundLayer;   // เลเยอร์พื้น

    private Transform player;
    private Rigidbody2D rb;
    private bool isHoming = false; // สถานะเริ่มติดตามผู้เล่น
    private bool isExploding = false;
    private float startTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        startTime = Time.time;

        // 1. ให้แรงผลักต้นตอนเกิด (พุ่งขึ้นและเฉียงไปข้างหน้าตามทิศที่มอนหัน)
        float spawnDirection = transform.right.x; // เช็คว่า firePoint หันไปทางไหน (1 หรือ -1)
        Vector2 initialBoost = new Vector2(forwardForce * spawnDirection, upwardForce);
        rb.AddForce(initialBoost, ForceMode2D.Impulse);

        // 2. ตั้งเวลาเริ่ม Homing
        Invoke("StartHoming", freeFlightDuration);

        // 3. ตั้งเวลาทำลายตัวเอง
        Invoke("Explode", lifeTime);
    }

    void StartHoming()
    {
        isHoming = true;
        // เมื่อเริ่ม Homing ให้เปลี่ยนเป็น Velocity Control แทน Impulse
        rb.angularVelocity = 0f; // เคลียร์แรงหมุนเก่า
    }

    void FixedUpdate()
    {
        if (isExploding || player == null || !isHoming) return;

        // --- ระบบ Homing ---

        // 🔥 1. กำหนดจุดเล็งเป้าหมายใหม่ (เอาตำแหน่งเท้า + ความสูงขึ้นมากลางลำตัว)
        Vector2 targetPos = new Vector2(player.position.x, player.position.y + targetOffsetY);

        // 2. หาทิศทางไปหาเป้าหมายใหม่ที่ยกสูงขึ้นแล้ว
        Vector2 direction = targetPos - rb.position;
        direction.Normalize();

        // 3. คำนวณองศาที่จรวดควรจะหันไป
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;

        // 4. หมุนหัวจรวดแบบ Smooth หาเป้าหมาย
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);

        // 5. บินไปข้างหน้า
        Vector2 moveDirection = new Vector2(
            Mathf.Cos((newAngle - angleOffset) * Mathf.Deg2Rad),
            Mathf.Sin((newAngle - angleOffset) * Mathf.Deg2Rad)
        );

        rb.linearVelocity = moveDirection * homingSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploding) return;

        // ชนผู้เล่น
        if (collision.CompareTag("Player"))
        {
            SplashX_PlayerStats stats = collision.GetComponent<SplashX_PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
            Explode();
        }
        // ชนพื้น (ถ้ามิสไซล์ยังบินฟรีอยู่ หรือ Homing อยู่แล้ว)
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // ถ้าชนพื้นตอนกำลังบินขึ้นฟรี ให้ระเบิดทันที
            Explode();
        }
    }

    void Explode()
    {
        if (isExploding) return;
        isExploding = true;

        if (explosionVFX != null) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Destroy(gameObject);
    }
}