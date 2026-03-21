using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_RangedMissileAI : MonoBehaviour
{
    [Header("Activation & Aggro")]
    public float activationRange = 20f;   // วงเขตที่เข้าแล้วมอนจะตื่น (Active)
    public float deactivationRange = 25f; // ระยะหนีห่างจนมอนเลิกตาม (หลับ)
    private bool isAwake = false;         // สถานะตื่นหรือยัง

    [Header("Movement & Positioning")]
    public float moveSpeed = 3f;
    public float preferredDistance = 8f;
    public float retreatDistance = 4f;

    [Header("Environment Checks (เช็คขอบเหว & กำแพง)")]
    public float wallCheckDistance = 0.6f;
    public float edgeCheckDistance = 1.5f;
    public float raycastOffsetY = 0.5f;
    public float edgeCheckOffsetX = 0.8f;
    public LayerMask groundLayer;

    [Header("Random Jump")]
    public float jumpForce = 12f;
    public float jumpCheckInterval = 2f;
    [Range(0f, 1f)] public float jumpChance = 0.3f;

    [Header("Missile Attack")]
    public GameObject missilePrefab;
    public Transform firePoint;
    public float fireCooldown = 4f;
    private float fireTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private SplashX_Enemy enemyStats;
    private Transform player;

    private bool isGrounded;
    private bool isAttacking = false;
    private bool facingRight = true;
    private bool isDead = false;
    private float jumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyStats = GetComponent<SplashX_Enemy>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        fireTimer = fireCooldown;
        jumpTimer = jumpCheckInterval;
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

        // 🔥 1. ระบบปลุก AI (เข้าวง = ตื่น, ออกนอกวงไกลๆ = หลับ)
        if (!isAwake)
        {
            if (distanceToPlayer <= activationRange)
            {
                isAwake = true; // ตื่น! เริ่มทำงาน
            }
            else
            {
                StopMoving(); // อยู่นอกวง ปล่อยยืนนิ่งๆ
                return;       // ตัดจบโค้ดตรงนี้ AI จะไม่เปลืองทรัพยากรคำนวณ
            }
        }
        else
        {
            // ถ้าตื่นอยู่ แต่วิ่งหนีออกไปไกลเกินระยะ เลิกตาม
            if (distanceToPlayer > deactivationRange)
            {
                isAwake = false;
                StopMoving();
                return;
            }
        }

        // 2. เช็คการเหยียบพื้น
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, edgeCheckDistance, groundLayer);

        fireTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;

        // 3. สุ่มกระโดด
        if (jumpTimer <= 0)
        {
            if (isGrounded && Random.value <= jumpChance && !isAttacking)
            {
                Jump();
            }
            jumpTimer = jumpCheckInterval;
        }

        // 4. ระบบรักษาระยะและโจมตี (ทำงานเมื่อตื่นแล้วเท่านั้น)
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
                StopMoving(); // ระยะพอดี ยืนนิ่งเตรียมยิง
                if (fireTimer <= 0 && isGrounded)
                {
                    StartCoroutine(ShootMissileRoutine());
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

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator ShootMissileRoutine()
    {
        isAttacking = true;
        StopMoving();

        anim.SetTrigger("missle");
        yield return new WaitForSeconds(0.5f);

        if (missilePrefab != null && firePoint != null && !enemyStats.isStunned)
        {
            Instantiate(missilePrefab, firePoint.position, firePoint.rotation);
        }

        yield return new WaitForSeconds(0.5f);
        fireTimer = fireCooldown;
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("death");
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, activationRange); // เส้นวงปลุก AI สีม่วงแดง

        if (Application.isPlaying)
        {
            float dirX = facingRight ? 1f : -1f;
            Vector3 rayStartPos = transform.position + new Vector3(0, raycastOffsetY, 0);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(rayStartPos, rayStartPos + new Vector3(dirX * wallCheckDistance, 0, 0));

            Gizmos.color = Color.yellow;
            Vector3 edgeCheckPos = transform.position + new Vector3(dirX * edgeCheckOffsetX, raycastOffsetY, 0);
            Gizmos.DrawLine(edgeCheckPos, edgeCheckPos + Vector3.down * edgeCheckDistance);
        }
    }
}