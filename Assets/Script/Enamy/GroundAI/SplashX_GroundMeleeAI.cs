using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))] // ดึงระบบเลือดและการชะงักมาใช้ร่วมกัน
public class SplashX_GroundMeleeAI : MonoBehaviour
{
    [Header("Movement & Chase")]
    public float moveSpeed = 4f;
    public float chaseRange = 15f;
    public float stopDistance = 1.2f; // ระยะที่มันจะหยุดเดินแล้วง้างตี

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float jumpHeightThreshold = 1.5f; // ถ้าผู้เล่นอยู่สูงกว่าระยะนี้ มันจะกระโดด
    public float wallCheckDistance = 0.5f;   // ระยะเช็คกำแพงข้างหน้า

    [Header("Combat Settings")]
    public int attackDamage = 15;
    public float attackCooldown = 2f;
    public float attackRange = 1.5f;
    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private SplashX_Enemy enemyStats;
    private Transform player;

    private bool isGrounded;
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
    }

    void Update()
    {
        // 1. เช็คตาย
        if (enemyStats.currentHealth <= 0 && !isDead)
        {
            Die();
            return;
        }

        if (isDead || player == null) return;

        // 2. เช็คการชะงักจากการโดนตี (ดึงค่ามาจาก SplashX_Enemy.cs)
        if (enemyStats.isStunned)
        {
            anim.Play("Hurt"); // บังคับเล่นท่าเจ็บ
            return;
        }

        attackTimer -= Time.deltaTime;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        anim.SetBool("isGrounded", isGrounded);

        float distToPlayerX = Mathf.Abs(player.position.x - transform.position.x);
        float distToPlayerY = player.position.y - transform.position.y;
        float absoluteDist = Vector2.Distance(transform.position, player.position);

        // 3. ระบบ AI ตามล่าผู้เล่น
        if (absoluteDist <= chaseRange && !isAttacking)
        {
            FlipTowardsPlayer();

            // เช็คว่าถึงระยะโจมตีหรือยัง
            if (absoluteDist <= stopDistance)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);

                if (attackTimer <= 0)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                // เดินเข้าหา
                float dirX = facingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);

                // ระบบตัดสินใจกระโดด: ผู้เล่นอยู่สูงกว่า หรือ ติดกำแพงข้างหน้า
                bool hitWall = Physics2D.Raycast(transform.position, Vector2.right * dirX, wallCheckDistance, groundLayer);
                if (isGrounded && (distToPlayerY > jumpHeightThreshold || hitWall))
                {
                    Jump();
                }
            }
        }
        else if (!isAttacking)
        {
            // ผู้เล่นอยู่นอกระยะ ปล่อยยืนเฉยๆ
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump"); // ถ้ามีท่ากระโดดก็ใส่ Trigger "Jump" ไว้ใน Animator
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; // หยุดเดินตอนตี

        anim.SetTrigger("Attack"); // ส่งสัญญาณไป Animator ให้ลาก Any State -> ท่า Attack

        // รอให้จังหวะดาเมจออก (ปรับเวลาให้ตรงกับเฟรมที่ง้างสุด)
        yield return new WaitForSeconds(0.4f);

        // เช็คอีกรอบว่าตอนตี ผู้เล่นยังอยู่ในระยะไหม
        if (Vector2.Distance(transform.position, player.position) <= attackRange && !enemyStats.isStunned)
        {
            SplashX_PlayerStats pStats = player.GetComponent<SplashX_PlayerStats>();
            if (pStats != null) pStats.TakeDamage(attackDamage);
        }

        // รอจนจบแอนิเมชันตี
        yield return new WaitForSeconds(0.6f);

        attackTimer = attackCooldown;
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Death"); // Any State -> ท่า Death

        // สคริปต์ SplashX_Enemy ของคุณจัดการเรื่อง Destroy ทิ้งไว้แล้ว
        // เราแค่สั่งเล่นแอนิเมชันตอน HP เป็น 0 ก็พอ
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}
