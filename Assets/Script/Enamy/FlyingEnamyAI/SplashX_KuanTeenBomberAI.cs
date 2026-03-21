using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_KuanTeenBomberAI : MonoBehaviour
{
    public enum BomberState { Incoming, Aiming, Fleeing, Dead }
    [Header("Current State")]
    public BomberState currentState = BomberState.Incoming;

    [Header("Flight Settings")]
    public float flySpeed = 25f;
    public float fleeSpeed = 35f;
    public float hoverHeight = 5f;
    public float aimDuration = 0.3f;
    public float destroyDistance = 40f;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject warningVFX;

    private Rigidbody2D rb;
    private SplashX_Enemy enemyStats;
    private Transform player;
    private Vector2 fleeDirection;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<SplashX_Enemy>();

        // บังคับให้เป็นตัวบิน และมีเลือดแค่ 1 
        rb.gravityScale = 0f;
        enemyStats.maxHealth = 1;
        enemyStats.currentHealth = 1;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (warningVFX != null) warningVFX.SetActive(false);
    }

    void Update()
    {
        if (enemyStats.currentHealth <= 0 && currentState != BomberState.Dead)
        {
            Die();
            return;
        }

        if (player == null || currentState == BomberState.Dead) return;

        switch (currentState)
        {
            case BomberState.Incoming:
                IncomingLogic();
                break;
            case BomberState.Aiming:
                break;
            case BomberState.Fleeing:
                FleeLogic();
                break;
        }
    }

    void IncomingLogic()
    {
        Vector2 targetPos = new Vector2(player.position.x, player.position.y + hoverHeight);

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * flySpeed;

        FlipTowards(player.position.x);

        if (Vector2.Distance(transform.position, targetPos) < 1f)
        {
            StartCoroutine(ShootAndFleeRoutine());
        }
    }

    IEnumerator ShootAndFleeRoutine()
    {
        currentState = BomberState.Aiming;
        rb.linearVelocity = Vector2.zero;

        if (warningVFX != null) warningVFX.SetActive(true);

        Debug.Log("💣 [BOMBER] หยุดบนหัวแล้ว! กำลังเล็ง...");

        yield return new WaitForSeconds(aimDuration);

        if (warningVFX != null) warningVFX.SetActive(false);

        // 🔥 เช็คว่ามีของครบไหม
        if (projectilePrefab == null) Debug.LogError("❌ [ERROR] ลืมใส่ Prefab กระสุนโง่ๆ ในช่อง Projectile Prefab!");
        if (firePoint == null) Debug.LogError("❌ [ERROR] ลืมใส่จุดปล่อยกระสุน ในช่อง Fire Point!");

        // ยิงกระสุนดิ่งลงหัว
        if (projectilePrefab != null && firePoint != null)
        {
            Debug.Log("🚀 [BOMBER] ปล่อยระเบิดลงไปแล้ว!");

            // บังคับเสกกระสุนและให้มันโผล่หน้าสุด (Z=0) กันมันไปโผล่หลังฉาก
            Vector3 spawnPos = new Vector3(firePoint.position.x, firePoint.position.y, 0f);
            Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0, 0, -90f));
        }

        float dirX = facingRight ? 1f : -1f;
        fleeDirection = new Vector2(dirX, 1f).normalized;

        currentState = BomberState.Fleeing;
        Debug.Log("💨 [BOMBER] ทิ้งระเบิดเสร็จแล้ว ชิ่งล่ะจ้าาา!");
    }

    void FleeLogic()
    {
        rb.linearVelocity = fleeDirection * fleeSpeed;

        if (Vector2.Distance(transform.position, player.position) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    void Die()
    {
        currentState = BomberState.Dead;
        rb.linearVelocity = Vector2.zero;

        rb.gravityScale = 2f;

        if (warningVFX != null) warningVFX.SetActive(false);
        StopAllCoroutines();
    }

    void FlipTowards(float targetX)
    {
        if (targetX > transform.position.x && !facingRight)
        {
            facingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (targetX < transform.position.x && facingRight)
        {
            facingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }
}