using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SplashX_Enemy))]
// ลบ RequireComponent Animator ออกไปแล้ว!
public class SplashX_FlyingShooterAI : MonoBehaviour
{
    public enum FlyingState { Hover, ShootWindUp, DashWindUp, Dashing, Cooldown, Dead }
    [Header("Current State")]
    public FlyingState currentState = FlyingState.Hover;

    [Header("Hover & Positioning (บินวน)")]
    public float hoverSpeed = 4f;
    public float hoverDistMin = 3f;
    public float hoverDistMax = 8f;
    public float hoverHeightMin = 1.5f;
    public float hoverHeightMax = 3.5f;
    public float changePosInterval = 2f;
    private Vector2 targetHoverPos;
    private float changePosTimer;

    [Header("Combat Flow")]
    public float attackCooldown = 3f;
    [Range(0f, 1f)] public float dashProbability = 0.6f;
    private float attackTimer;

    [Header("Dash Attack (พุ่งชน)")]
    public float dashSpeed = 18f;
    public float dashWindUp = 0.5f;
    public float dashDuration = 0.4f;
    public int dashDamage = 15;
    public GameObject dashChargeVFX; // 🔥 เอฟเฟกต์ชาร์จพลังก่อนพุ่งชน (ใส่เป็น Object ลูก)
    private Vector2 dashDirection;
    private bool hasDamagedThisDash = false;

    [Header("Shoot Attack (ยิงกระสุน)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootWindUp = 0.5f;
    public GameObject shootChargeVFX; // 🔥 เอฟเฟกต์แสงรวมพลังก่อนยิง (ใส่เป็น Object ลูก)

    [Header("Death Effects")]
    public GameObject deathVFX; // 🔥 เอฟเฟกต์ระเบิดตอนตาย (Prefab)

    private Rigidbody2D rb;
    private SplashX_Enemy enemyStats;
    private Transform player;

    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<SplashX_Enemy>();

        rb.gravityScale = 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // ปิด VFX ชาร์จไว้ก่อนตอนเริ่มเกม
        if (shootChargeVFX != null) shootChargeVFX.SetActive(false);
        if (dashChargeVFX != null) dashChargeVFX.SetActive(false);

        attackTimer = attackCooldown;
        PickNewHoverPoint();
    }

    void Update()
    {
        if (enemyStats.currentHealth <= 0 && currentState != FlyingState.Dead)
        {
            Die();
            return;
        }

        if (player == null || currentState == FlyingState.Dead) return;

        if (enemyStats.isStunned)
        {
            StopAllAttacks(); // โดนตีปุ๊บ ยกเลิกการชาร์จ/ยิง ทันที
            return;
        }

        switch (currentState)
        {
            case FlyingState.Hover:
                HoverLogic();
                FlipTowardsPlayer();
                break;
            case FlyingState.Dashing:
                break;
            case FlyingState.Cooldown:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 5f);
                FlipTowardsPlayer();
                break;
        }
    }

    void HoverLogic()
    {
        changePosTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (changePosTimer <= 0) PickNewHoverPoint();

        Vector2 actualTargetPos = (Vector2)player.position + targetHoverPos;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, (actualTargetPos - (Vector2)transform.position) * hoverSpeed, Time.deltaTime * 3f);

        if (attackTimer <= 0)
        {
            if (Random.value <= dashProbability)
                StartCoroutine(DashRoutine());
            else
                StartCoroutine(ShootRoutine());
        }
    }

    void PickNewHoverPoint()
    {
        float side = Random.value > 0.5f ? 1f : -1f;
        float randomX = Random.Range(hoverDistMin, hoverDistMax) * side;
        float randomY = Random.Range(hoverHeightMin, hoverHeightMax);

        targetHoverPos = new Vector2(randomX, randomY);
        changePosTimer = changePosInterval;
    }

    // --- ระบบพุ่งชน (Dash) ---
    IEnumerator DashRoutine()
    {
        currentState = FlyingState.DashWindUp;
        rb.linearVelocity = Vector2.zero;

        // 🔥 เปิดเอฟเฟกต์ชาร์จก่อนพุ่ง (เช่น แสงสีแดงกระพริบที่ตัว)
        if (dashChargeVFX != null) dashChargeVFX.SetActive(true);

        yield return new WaitForSeconds(dashWindUp);

        // 🔥 ปิดเอฟเฟกต์ชาร์จ
        if (dashChargeVFX != null) dashChargeVFX.SetActive(false);

        dashDirection = (player.position - transform.position).normalized;
        currentState = FlyingState.Dashing;
        hasDamagedThisDash = false;

        rb.linearVelocity = dashDirection * dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        currentState = FlyingState.Cooldown;
        yield return new WaitForSeconds(1f);

        attackTimer = attackCooldown;
        currentState = FlyingState.Hover;
    }

    // --- ระบบยิง (Shoot) ---
    IEnumerator ShootRoutine()
    {
        currentState = FlyingState.ShootWindUp;
        rb.linearVelocity = Vector2.zero;

        // 🔥 เปิดเอฟเฟกต์ชาร์จปืน (เช่น ลูกบอลพลังงานหดตัวตรงปลายกระบอก)
        if (shootChargeVFX != null) shootChargeVFX.SetActive(true);

        yield return new WaitForSeconds(shootWindUp);

        // 🔥 ปิดเอฟเฟกต์ชาร์จปืน
        if (shootChargeVFX != null) shootChargeVFX.SetActive(false);

        if (projectilePrefab != null && firePoint != null && !enemyStats.isStunned)
        {
            Vector2 dirToPlayer = player.position - firePoint.position;
            float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

            Instantiate(projectilePrefab, firePoint.position, bulletRotation);
        }

        currentState = FlyingState.Cooldown;
        yield return new WaitForSeconds(0.5f);

        attackTimer = attackCooldown;
        currentState = FlyingState.Hover;
    }

    void StopAllAttacks()
    {
        if (currentState == FlyingState.ShootWindUp || currentState == FlyingState.DashWindUp || currentState == FlyingState.Dashing)
        {
            StopAllCoroutines();
            if (shootChargeVFX != null) shootChargeVFX.SetActive(false);
            if (dashChargeVFX != null) dashChargeVFX.SetActive(false);
            currentState = FlyingState.Cooldown;
            attackTimer = attackCooldown;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == FlyingState.Dashing && collision.CompareTag("Player") && !hasDamagedThisDash)
        {
            hasDamagedThisDash = true;
            SplashX_PlayerStats pStats = collision.GetComponent<SplashX_PlayerStats>();
            if (pStats != null) pStats.TakeDamage(dashDamage);
        }
    }

    void Die()
    {
        currentState = FlyingState.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 2f; // ร่วงลงพื้น

        StopAllCoroutines();
        if (shootChargeVFX != null) shootChargeVFX.SetActive(false);
        if (dashChargeVFX != null) dashChargeVFX.SetActive(false);

        // 🔥 เสกควันระเบิดแทนการเล่นท่าตาย
        if (deathVFX != null) Instantiate(deathVFX, transform.position, Quaternion.identity);

        // SplashX_Enemy.cs มีการเรียก Wait 2 วิ แล้วค่อยลบตัวเองอยู่แล้ว
        // ปล่อยให้ศพมันร่วงลงพื้นไปแบบฟิสิกส์เน้นๆ ได้เลย
    }

    void FlipTowardsPlayer()
    {
        if (player.position.x > transform.position.x && !facingRight)
        {
            facingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            facingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }
}