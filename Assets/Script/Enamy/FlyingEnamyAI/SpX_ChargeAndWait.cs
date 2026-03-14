using UnityEngine;
using System.Collections;

public class SplashX_DemonEyeAI : MonoBehaviour
{
    public enum EyeState { Hover, WindUp, Dash, Cooldown }
    private bool facingRight = true;
    [Header("Current State")]
    public EyeState currentState = EyeState.Hover;

    [Header("Components")]
    public TrailRenderer trail;
    public GameObject sparkEffectPrefab; // 💥 ลาก Prefab Sparks_VFX มาใส่ในช่องนี้ใน Inspector

    [Header("Hover & Positioning")]
    public float hoverSpeed = 4f;
    public float chaseRange = 15f;
    public float hoverHeightMin = 2f;
    public float hoverHeightMax = 5f;
    public float hoverWidth = 4f;
    public float attackCooldown = 3f;

    [Header("Dash Combat Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;
    public float windUpTime = 0.6f;
    public float cooldownTime = 1.5f;
    public int damage = 1;

    [Header("Obstacle Avoidance & Bounce")]
    public LayerMask obstacleLayer;
    public float avoidRange = 2f;
    public float bodyRadius = 0.5f; // 🟢 ขนาดตัวของลูกตาสำหรับเช็คเด้งกำแพง

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 dashDirection;
    private bool hasDamagedThisDash = false;

    private Vector2 currentHoverOffset;
    private float hoverChangeTimer;
    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (trail != null) trail.emitting = false;
        attackTimer = attackCooldown;
        PickNewHoverPoint();
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case EyeState.Hover:
                HoverLogic();
                break;
            case EyeState.WindUp:
                break;
            case EyeState.Dash:
                DashLogic(); // 🔥 เรียกใช้ระบบเช็คเด้งกำแพงตรงนี้
                break;
            case EyeState.Cooldown:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
                break;
        }
        RotateTowardsVelocity();
    }
    void HoverLogic()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > chaseRange)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 5f);
            return;
        }

        attackTimer -= Time.deltaTime;
        hoverChangeTimer -= Time.deltaTime;

        if (hoverChangeTimer <= 0) PickNewHoverPoint();

        Vector2 targetPos = (Vector2)player.position + currentHoverOffset;
        Vector2 dirToTarget = (targetPos - (Vector2)transform.position).normalized;

        Vector2 safeDirection = FindClearPath(dirToTarget);
        rb.linearVelocity = safeDirection * hoverSpeed;

        if (attackTimer <= 0)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void PickNewHoverPoint()
    {
        float randomX = Random.Range(-hoverWidth, hoverWidth);
        float randomY = Random.Range(hoverHeightMin, hoverHeightMax);
        currentHoverOffset = new Vector2(randomX, randomY);
        hoverChangeTimer = Random.Range(0.5f, 1.5f);
    }

    Vector2 FindClearPath(Vector2 targetDir)
    {
        if (!Physics2D.Raycast(transform.position, targetDir, avoidRange, obstacleLayer))
            return targetDir;

        for (int angle = 15; angle <= 90; angle += 15)
        {
            Vector2 dirRight = Quaternion.Euler(0, 0, angle) * targetDir;
            if (!Physics2D.Raycast(transform.position, dirRight, avoidRange, obstacleLayer))
                return dirRight;

            Vector2 dirLeft = Quaternion.Euler(0, 0, -angle) * targetDir;
            if (!Physics2D.Raycast(transform.position, dirLeft, avoidRange, obstacleLayer))
                return dirLeft;
        }

        return -targetDir;
    }

    // --- 🟢 ระบบเช็คเด้งกำแพง (Raycast Reflection) ---
    void DashLogic()
    {
        // ยิงเรดาร์วงกลมไปข้างหน้าล่วงหน้า 1 เฟรม
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, dashDirection, (dashSpeed * Time.deltaTime) + 0.1f, obstacleLayer);

        if (hit.collider != null)
        {
            // --- 🟢 ส่วนที่ 2: ยิงเอฟเฟกต์สะเก็ดไฟ ---
            if (sparkEffectPrefab != null)
            {
                // หาพิกัดที่ชน และ ทิศทางตั้งฉากของกำแพง
                Vector2 contactPoint = hit.point;
                Vector2 wallNormal = hit.normal; // wallNormal คือทิศที่พุ่งออกจากกำแพง

                // คำนวณองศาให้เอฟเฟกต์พุ่งสวนทางออกมาจากกำแพง
                float angle = Mathf.Atan2(wallNormal.y, wallNormal.x) * Mathf.Rad2Deg;
                Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

                // สร้างเอฟเฟกต์ ณ จุดที่ชน พร้อมหมุนให้ถูกทิศ
                Instantiate(sparkEffectPrefab, contactPoint, spawnRotation);
            }
            // ----------------------------------------

            // คำนวณมุมตกกระทบแล้วเด้งออก (Reflect)
            dashDirection = Vector2.Reflect(dashDirection, hit.normal);

            // สั่งให้พุ่งไปในทิศทางใหม่ที่เพิ่งเด้งมา
            rb.linearVelocity = dashDirection * dashSpeed;
        }
    }

    IEnumerator DashRoutine()
    {
        currentState = EyeState.WindUp;
        rb.linearVelocity = Vector2.zero; // เบรกหยุดนิ่งเพื่อชาร์จ

        // รอเวลาชาร์จ (จังหวะนี้มันจะนิ่งๆ ให้ผู้เล่นเตรียมตัว)
        yield return new WaitForSeconds(windUpTime);

        // 🔥 จุดเปลี่ยนความโหด: ล็อคเป้าหมาย ณ เสี้ยววินาทีสุดท้ายก่อนออกตัว!
        dashDirection = (player.position - transform.position).normalized;

        currentState = EyeState.Dash;
        hasDamagedThisDash = false;
        if (trail != null) trail.emitting = true;

        // สับสปีดพุ่งใส่ตำแหน่งที่ล็อคไว้
        rb.linearVelocity = dashDirection * dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        if (trail != null) trail.emitting = false;
        currentState = EyeState.Cooldown;
        yield return new WaitForSeconds(cooldownTime);

        attackTimer = attackCooldown;
        currentState = EyeState.Hover;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == EyeState.Dash && collision.CompareTag("Player") && !hasDamagedThisDash)
        {
            hasDamagedThisDash = true;

            SplashX_PlayerStats playerStats = collision.GetComponent<SplashX_PlayerStats>();
            if (playerStats != null) playerStats.TakeDamage(damage);

            Debug.Log("Dash ชนผู้เล่น! ลดเลือด " + damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = player.position + new Vector3(0, (hoverHeightMin + hoverHeightMax) / 2f, 0);
            Vector3 size = new Vector3(hoverWidth * 2f, hoverHeightMax - hoverHeightMin, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
    // --- ระบบหมุนหัวสว่านชี้ตามทิศทางความเร็ว ---
    void RotateTowardsVelocity()
    {
        // เช็คว่ามีความเร็วอยู่ไหม (เพื่อไม่ให้มันรีเซ็ตหันขวาตอนเบรกจอดนิ่งๆ)
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            // คำนวณองศาจากแกน X และ Y ของความเร็ว (Atan2 คือสูตรหาองศาจากเวกเตอร์)
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;

            // สั่งหมุน Object ไปตามองศานั้นในแกน Z
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}