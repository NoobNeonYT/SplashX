using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_GroundMeleeAI : MonoBehaviour
{
    [Header("Activation & Aggro")]
    public float activationRange = 20f;   // ระยะเข้าวงเพื่อปลุก AI
    public float deactivationRange = 25f; // ระยะหนีห่างเพื่อทิ้งโฟกัส (หลับ)
    private bool isAwake = false;

    [Header("Movement & Chase")]
    public float moveSpeed = 4f;
    public float chaseRange = 15f;        // ระยะที่เริ่มวิ่งไล่ล่า (ควรน้อยกว่า activationRange)
    public float stopDistance = 1.2f;     // ระยะง้างดาบตี

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float jumpHeightThreshold = 1.5f;
    public float wallCheckDistance = 0.5f;

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

        float absoluteDist = Vector2.Distance(transform.position, player.position);

        // 🔥 1. ระบบปลุก AI (เข้าวง = ตื่น, ออกนอกวงไกลๆ = หลับ)
        if (!isAwake)
        {
            if (absoluteDist <= activationRange)
            {
                isAwake = true; // ตื่น! เริ่มทำงาน
            }
            else
            {
                return; // ตัดจบโค้ดตรงนี้ AI จะไม่เปลืองทรัพยากรคำนวณเลยถ้าเราอยู่ไกล
            }
        }
        else
        {
            if (absoluteDist > deactivationRange)
            {
                isAwake = false; // กลับไปหลับ
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);
                return;
            }
        }

        // 2. ถ้าตื่นแล้ว ค่อยคำนวณการเดินและการโจมตีต่อ
        attackTimer -= Time.deltaTime;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // ถ้า Animator ของคุณยังไม่มี isGrounded ให้ปล่อยคอมเมนต์บรรทัดนี้ไว้นะครับ
        // anim.SetBool("isGrounded", isGrounded); 

        float distToPlayerY = player.position.y - transform.position.y;

        // 3. ระบบไล่ล่าและโจมตี
        if (absoluteDist <= chaseRange && !isAttacking)
        {
            FlipTowardsPlayer();

            if (absoluteDist <= stopDistance)
            {
                // ถึงระยะง้างตี
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);

                if (attackTimer <= 0)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                // เดินไล่ฟัน
                float dirX = facingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);

                // เช็คกำแพง & ผู้เล่นอยู่สูงกว่า -> สั่งโดด
                bool hitWall = Physics2D.Raycast(transform.position, Vector2.right * dirX, wallCheckDistance, groundLayer);
                if (isGrounded && (distToPlayerY > jumpHeightThreshold || hitWall))
                {
                    Jump();
                }
            }
        }
        else if (!isAttacking)
        {
            // อยู่นอกระยะวิ่งไล่ แต่ยังตื่นอยู่ (ยืนนิ่งๆ มองตาม)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // ถ้า Animator ของคุณยังไม่มี Trigger ชื่อ Jump ให้คอมเมนต์บรรทัดนี้ไว้ก่อน
        // anim.SetTrigger("Jump"); 
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        if (Vector2.Distance(transform.position, player.position) <= attackRange && !enemyStats.isStunned)
        {
            SplashX_PlayerStats pStats = player.GetComponent<SplashX_PlayerStats>();
            if (pStats != null) pStats.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.6f);

        attackTimer = attackCooldown;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, activationRange); // วงปลุก

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange); // วงวิ่งใส่

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance); // วงง้างดาบ

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}