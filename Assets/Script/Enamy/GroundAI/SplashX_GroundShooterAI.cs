using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_GroundShooterAI : MonoBehaviour
{
    [Header("Activation & Aggro")]
    public float activationRange = 20f;   // ระยะตื่น
    public float deactivationRange = 25f; // ระยะหลับ
    private bool isAwake = false;

    [Header("Movement & Positioning")]
    public float moveSpeed = 3f;
    public float preferredDistance = 7f;  // ระยะยืนยิงที่ชอบ
    public float retreatDistance = 3f;    // ถอยหนีถ้าเคียวเข้ามาใกล้เกิน

    [Header("Environment Checks")]
    public float wallCheckDistance = 0.6f;
    public float edgeCheckDistance = 1.5f;
    public float raycastOffsetY = 0.5f;
    public float edgeCheckOffsetX = 0.8f;
    public LayerMask groundLayer;

    [Header("Ranged Attack (กระสุนพุ่งตรง)")]
    public GameObject projectilePrefab; // 🔥 ลาก Prefab ที่ใส่สคริปต์ SplashX_Projectile มาใส่ช่องนี้
    public Transform firePoint;
    public float fireCooldown = 1.5f;   // ยิงรัวกว่ามิสไซล์หน่อย
    private float fireTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private SplashX_Enemy enemyStats;
    private Transform player;

    private bool isAttacking = false;
    private bool facingRight = true;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyStats = GetComponent<SplashX_Enemy>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        fireTimer = fireCooldown;
    }

    void Update()
    {
        if (enemyStats.currentHealth <= 0 && !isDead)
        {
            Die();
            return;
        }

        if (isDead || player == null) return;

        if (enemyStats.isStunned)
        {
            anim.Play("Hurt");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. ระบบปลุก AI
        if (!isAwake)
        {
            if (distanceToPlayer <= activationRange) isAwake = true;
            else return;
        }
        else if (distanceToPlayer > deactivationRange)
        {
            isAwake = false;
            StopMoving();
            return;
        }

        fireTimer -= Time.deltaTime;

        // 2. ระบบรักษาระยะและยิง
        if (!isAttacking)
        {
            FlipTowardsPlayer();

            if (distanceToPlayer > preferredDistance)
            {
                Move(1f); // เดินหน้าตาม
            }
            else if (distanceToPlayer < retreatDistance)
            {
                Move(-1f); // ถอยหลังหนี
            }
            else
            {
                StopMoving(); // ระยะพอดี ยืนเล็งยิง

                // เช็คว่าเหยียบพื้นอยู่ไหมก่อนยิง (กันมันยิงออกมาระหว่างกำลังร่วงหล่น)
                bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, edgeCheckDistance, groundLayer);
                if (fireTimer <= 0 && isGrounded)
                {
                    StartCoroutine(ShootRoutine());
                }
            }
        }
    }

    void Move(float directionMultiplier)
    {
        float dirX = (facingRight ? 1f : -1f) * directionMultiplier;

        Vector2 rayStartPos = new Vector2(transform.position.x, transform.position.y + raycastOffsetY);
        bool hitWall = Physics2D.Raycast(rayStartPos, Vector2.right * dirX, wallCheckDistance, groundLayer);

        Vector2 edgeCheckPos = new Vector2(transform.position.x + (dirX * edgeCheckOffsetX), transform.position.y + raycastOffsetY);
        bool hasGroundAhead = Physics2D.Raycast(edgeCheckPos, Vector2.down, edgeCheckDistance, groundLayer);

        if (hitWall || !hasGroundAhead)
        {
            StopMoving();
        }
        else
        {
            rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
            anim.SetBool("isWalking", true);
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isWalking", false);
    }

    IEnumerator ShootRoutine()
    {
        isAttacking = true;
        StopMoving();

        // 🔥 สั่งเล่นท่ายิง (ไปตั้ง Trigger "Shoot" ใน Animator ด้วยนะครับ)
        anim.SetTrigger("Shoot");

        // รอจังหวะง้างปืน/ธนู (ปรับเวลาให้ตรงกับเฟรมที่กระสุนควรพุ่งออกไป)
        yield return new WaitForSeconds(0.3f);

        if (projectilePrefab != null && firePoint != null && !enemyStats.isStunned)
        {
            // เสกกระสุน! สคริปต์ SplashX_Projectile ของคุณจะจัดการให้มันพุ่งตรงไปข้างหน้าเอง
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }

        // รอจนจบท่ายิง
        yield return new WaitForSeconds(0.4f);

        fireTimer = fireCooldown;
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Death");
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