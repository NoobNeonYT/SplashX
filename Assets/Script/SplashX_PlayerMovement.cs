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

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody2D rb;
    private float moveInput;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) return;

        // 1. รับค่าการเดิน
        moveInput = Input.GetAxisRaw("Horizontal");

        // 2. เช็คพื้น
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 3. กระโดด (Jump)
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            // แก้ velocity -> linearVelocity
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. แดช (Dash)
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(Dash());
        }

        Flip();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // แก้ velocity -> linearVelocity
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

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

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // แก้ velocity -> linearVelocity
        rb.linearVelocity = new Vector2((facingRight ? 1 : -1) * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; // หยุดตัวละครหลัง Dash เสร็จนิดนึง
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}