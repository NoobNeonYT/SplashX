using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SplashX_Enemy))]
public class SplashX_GroundMeleeAI : MonoBehaviour
{
    [Header("Activation & Aggro")]
    public float activationRange = 20f;
    public float deactivationRange = 25f;
    private bool isAwake = false;

    [Header("Movement & Chase")]
    public float moveSpeed = 4f;
    public float chaseRange = 15f;
    public float stopDistance = 1.2f;

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

        // 🔥 ปิดระบบชะงัก (Stun) ทิ้งไปเลย! มอนตัวนี้โดนตีก็ยังเดินหน้าบวกต่อ ไม่ยืนเอ๋อให้ตีฟรีแน่นอน
        /* if (enemyStats.isStunned)
        {
            anim.Play("Hurt"); 
            return;
        }
        */

        float absoluteDist = Vector2.Distance(transform.position, player.position);

        if (!isAwake)
        {
            if (absoluteDist <= activationRange) isAwake = true;
            else return;
        }
        else
        {
            if (absoluteDist > deactivationRange)
            {
                isAwake = false;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);
                return;
            }
        }

        attackTimer -= Time.deltaTime;

        // เช็คพื้น ถ้าคุณยังไม่มี object groundCheck ให้ปล่อยคอมเมนต์ไว้
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        // anim.SetBool("isGrounded", isGrounded); 

        float distToPlayerY = player.position.y - transform.position.y;

        if (absoluteDist <= chaseRange && !isAttacking)
        {
            FlipTowardsPlayer(); // หันหน้าหาผู้เล่น (อัปเกรดแล้ว ไม่หันรัวๆ แน่นอน)

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
                float dirX = facingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);

                bool hitWall = Physics2D.Raycast(transform.position, Vector2.right * dirX, wallCheckDistance, groundLayer);
                if (isGrounded && (distToPlayerY > jumpHeightThreshold || hitWall))
                {
                    Jump();
                }
            }
        }
        else if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        // anim.SetTrigger("Jump"); 
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        // 🔥 เอาเงื่อนไข !enemyStats.isStunned ออกด้วยตอนทำดาเมจ จะได้ฟันสวนได้เลย
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
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
        float diffX = player.position.x - transform.position.x;

        // 🔥 ระบบ Deadzone: ต้องห่างกันแกน X เกิน 0.1 หน่วย ถึงจะยอมหันหน้า (แก้บัคยืนซ้อนทับกันแล้วหน้าสั่น)
        if (Mathf.Abs(diffX) > 0.1f)
        {
            if (diffX > 0 && !facingRight)
            {
                facingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else if (diffX < 0 && facingRight)
            {
                facingRight = false;
                transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, activationRange);

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