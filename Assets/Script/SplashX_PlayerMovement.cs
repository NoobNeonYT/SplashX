using System.Collections;
using UnityEngine;

public class SplashX_PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing;
    private bool canDash = true;

    [Header("Controls")]
    public KeyCode jumpKey = KeyCode.J;
    public KeyCode dashKey = KeyCode.K;
    public KeyCode downKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.Y;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 20;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float moveInput;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDashing) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 1. ระบบกระโดด และ ลงจากแพลตฟอร์ม
        if (Input.GetKeyDown(jumpKey))
        {
            if (Input.GetKey(downKey))
            {
                DropFromPlatform();
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        // 2. ระบบแดช
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(Dash());
        }

        // 3. ระบบโจมตี
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(attackKey))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        Flip();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- ฟังก์ชันโจมตี ---
    void Attack()
    {
        Debug.Log("สวิงกีตาร์แล้ว!");

        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        Debug.Log("เจอศัตรูในวงกลมจำนวน: " + hitEnemies.Length + " ตัว");

        foreach (Collider2D enemy in hitEnemies)
        {
            SplashX_Enemy enemyScript = enemy.GetComponent<SplashX_Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    // --- หันหน้าตัวละคร ---
    private void Flip()
    {
        if (moveInput > 0 && !facingRight)
        {
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (moveInput < 0 && facingRight)
        {
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // --- ฟังก์ชันลงจากแพลตฟอร์ม ---
    private void DropFromPlatform()
    {
        if (groundCheck == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);

        foreach (Collider2D col in colliders)
        {
            if (col.GetComponent<PlatformEffector2D>() != null)
            {
                StartCoroutine(DisableCollisionTemporary(col));
                break;
            }
        }
    }

    // --- Coroutine สำหรับปิดการชนตอนลงจากแพลตฟอร์มชั่วคราว ---
    private IEnumerator DisableCollisionTemporary(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    // --- Coroutine สำหรับพุ่ง (Dash) ---
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2((facingRight ? 1 : -1) * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // --- วาดเส้นช่วยเล็งในหน้า Scene ---
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}