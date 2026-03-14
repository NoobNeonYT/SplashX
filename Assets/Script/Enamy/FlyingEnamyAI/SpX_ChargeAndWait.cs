using UnityEngine;
using System.Collections;

public class SplashX_DemonEyeAI : MonoBehaviour
{
    public enum EyeState { Hover, WindUp, Dash, Cooldown }
    [Header("Current State")]
    public EyeState currentState = EyeState.Hover;

    [Header("Components")]
    public TrailRenderer trail;
    public GameObject sparkEffectPrefab;

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
    public float bodyRadius = 0.5f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 dashDirection;
    private bool hasDamagedThisDash = false;

    private Vector2 currentHoverOffset;
    private float hoverChangeTimer;
    private float attackTimer;

    // ตัวแปรสำหรับเช็คการหันหน้า
    private bool facingRight = true;

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
                DashLogic();
                break;
            case EyeState.Cooldown:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
                break;
        }

        // เรียกใช้ฟังก์ชันหันซ้ายขวา
        FlipEye();
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
        float distToTarget = Vector2.Distance(transform.position, targetPos);

        if (distToTarget < 0.2f)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 10f);
        }
        else
        {
            Vector2 dirToTarget = (targetPos - (Vector2)transform.position).normalized;
            Vector2 safeDirection = FindClearPath(dirToTarget);
            rb.linearVelocity = safeDirection * hoverSpeed;
        }

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

    void DashLogic()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, dashDirection, (dashSpeed * Time.deltaTime) + 0.1f, obstacleLayer);

        if (hit.collider != null)
        {
            if (sparkEffectPrefab != null)
            {
                Vector2 contactPoint = hit.point;
                Vector2 wallNormal = hit.normal;

                float angle = Mathf.Atan2(wallNormal.y, wallNormal.x) * Mathf.Rad2Deg;
                Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

                Instantiate(sparkEffectPrefab, contactPoint, spawnRotation);
            }

            dashDirection = Vector2.Reflect(dashDirection, hit.normal);
            rb.linearVelocity = dashDirection * dashSpeed;
        }
    }

    IEnumerator DashRoutine()
    {
        currentState = EyeState.WindUp;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(windUpTime);

        dashDirection = (player.position - transform.position).normalized;

        currentState = EyeState.Dash;
        hasDamagedThisDash = false;
        if (trail != null) trail.emitting = true;

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

    // --- ระบบหันหน้า ซ้าย-ขวา ---
    void FlipEye()
    {
        // เช็คความเร็วแกน X เพื่อสลับหน้า
        if (rb.linearVelocity.x > 0.1f && !facingRight)
        {
            facingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (rb.linearVelocity.x < -0.1f && facingRight)
        {
            facingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }

        // เพื่อป้องกันไม่ให้หัวมันหมุนเอียง (กรณีที่โค้ดเก่าทำค้างไว้)
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}