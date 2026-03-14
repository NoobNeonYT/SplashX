using UnityEngine;
using System.Collections;

public class SpX_ChargeAndWait : MonoBehaviour
{
    public enum EyeState { Hover, WindUp, Dash, Cooldown }

    [Header("Current State")]
    public EyeState currentState = EyeState.Hover;

    [Header("Movement & Combat Settings")]
    public float hoverSpeed = 3f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;
    public float windUpTime = 0.6f;  // เวลาชาร์จก่อนพุ่ง
    public float cooldownTime = 1.5f; // เวลาบินรำหลังพุ่ง
    public float attackRange = 6f;    // ระยะที่จะเริ่มชาร์จ
    public int damage = 1;

    [Header("Obstacle Avoidance (กวาดหาทางอ้อม)")]
    public LayerMask obstacleLayer; // เลเยอร์กำแพง/พื้น
    public float avoidRange = 2f;   // ระยะสแกนกำแพง

    [Header("Components")]
    public TrailRenderer trail;     // ลาก Trail Renderer มาใส่

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 dashDirection;
    private bool hasDamagedThisDash = false; // เช็คว่าชนผู้เล่นไปหรือยังในรอบนี้

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (trail != null) trail.emitting = false; // ปิด Trail ไว้ก่อน
    }

    void Update()
    {
        if (player == null) return;

        // ควบคุม State [If A then B]
        switch (currentState)
        {
            case EyeState.Hover:
                HoverLogic();
                break;
            case EyeState.WindUp:
                // รอชาร์จ (โค้ดอยู่ใน Coroutine)
                break;
            case EyeState.Dash:
                // พุ่งทะลุไปเลย (โค้ดอยู่ใน Coroutine)
                break;
            case EyeState.Cooldown:
                // บินรำหาจังหวะ (ความเร็วลดลง)
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
                break;
        }
    }

    // --- 1. สถานะบินวน และหาทางอ้อมกำแพง ---
    void HoverLogic()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // ถ้าอยู่ในระยะ ให้เริ่มชาร์จ
        if (distToPlayer <= attackRange)
        {
            StartCoroutine(DashRoutine());
            return;
        }

        // หาทางบินไปหาผู้เล่นโดยหลบกำแพง
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        Vector2 safeDirection = FindClearPath(dirToPlayer);

        rb.linearVelocity = safeDirection * hoverSpeed;
    }

    // ระบบกวาดเรดาร์หาทางอ้อมกำแพงแบบสั้นที่สุด
    Vector2 FindClearPath(Vector2 targetDir)
    {
        // 1. เช็คตรงๆ ก่อน ถ้าว่างก็ไปเลย
        if (!Physics2D.Raycast(transform.position, targetDir, avoidRange, obstacleLayer))
            return targetDir;

        // 2. ถ้าติดกำแพง ให้กวาดองศาซ้าย-ขวา (ทีละ 15 องศา) เพื่อหาช่องว่างที่ใกล้ที่สุด
        for (int angle = 15; angle <= 90; angle += 15)
        {
            Vector2 dirRight = Quaternion.Euler(0, 0, angle) * targetDir;
            if (!Physics2D.Raycast(transform.position, dirRight, avoidRange, obstacleLayer))
                return dirRight; // เจอทางขวาก่อน เลี้ยวขวา!

            Vector2 dirLeft = Quaternion.Euler(0, 0, -angle) * targetDir;
            if (!Physics2D.Raycast(transform.position, dirLeft, avoidRange, obstacleLayer))
                return dirLeft;  // เจอทางซ้ายก่อน เลี้ยวซ้าย!
        }

        // ถ้าตันทุกทางจริงๆ ให้ถอยหลัง
        return -targetDir;
    }

    // --- 2. ลำดับการพุ่ง (WindUp -> Dash -> Cooldown) ---
    IEnumerator DashRoutine()
    {
        currentState = EyeState.WindUp;
        rb.linearVelocity = Vector2.zero; // หยุดนิ่งเพื่อชาร์จพลัง

        // กำหนดทิศทางล็อคเป้า
        dashDirection = (player.position - transform.position).normalized;

        // รอเวลา WindUp
        yield return new WaitForSeconds(windUpTime);

        // เริ่มพุ่ง (Dash)
        currentState = EyeState.Dash;
        hasDamagedThisDash = false; // รีเซ็ตการชน
        if (trail != null) trail.emitting = true; // เปิด Trail

        rb.linearVelocity = dashDirection * dashSpeed; // พุ่งตรงแหน่ว

        // รอจนกว่าจะหมดเวลาพุ่ง (พุ่งเลยไปเรื่อยๆ)
        yield return new WaitForSeconds(dashDuration);

        // หมดเวลาพุ่ง เข้าสู่ช่วงพัก (Cooldown)
        if (trail != null) trail.emitting = false; // ปิด Trail
        currentState = EyeState.Cooldown;

        // รอเวลาบินรำ
        yield return new WaitForSeconds(cooldownTime);

        // กลับไปบินตามปกติ
        currentState = EyeState.Hover;
    }

    // --- 3. ระบบชนแล้วลดเลือด ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ถ้าพุ่งอยู่ และ ชน Player และ ยังไม่ได้ลดเลือดในรอบพุ่งนี้
        if (currentState == EyeState.Dash && collision.CompareTag("Player") && !hasDamagedThisDash)
        {
            hasDamagedThisDash = true; // ล็อคไว้ไม่ให้ดาเมจเด้งรัวๆ

            //ตรวจสอบว่ามีสคริปต์เลือดของผู้เล่นไหม
            SplashX_PlayerStats playerStats = collision.GetComponent<SplashX_PlayerStats>();
            if (playerStats != null) playerStats.TakeDamage(damage);

            Debug.Log("Dash ชนผู้เล่น! ลดเลือด " + damage);
            // สังเกตว่าเราไม่ได้สั่ง rb.velocity = 0; มันเลยจะพุ่งทะลุตัวผู้เล่นต่อไปเลยตามที่คุณเซฟต้องการ
        }
    }

    // วาดเส้นรัศมีให้เห็นใน Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}